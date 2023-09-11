// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using ChatMessage = Azure.AI.OpenAI.ChatMessage;

namespace CustomerSupportServiceSample.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly ILogger logger;
        private readonly SearchClient searchClient;
        private readonly OpenAIClient openAIClient;
        private readonly string openAIDeploymentName;
        private const string SystemAnswerPrompt = """
        You are a customer support chatbot for Contoso Energy. You can also call customers when they request to talk on phone. Refer to the data source if their is a technical question. Otherwise, just continue the conversation in a friendly manner. Ask the customer follow up questions if there is an opportunity to do so to understand their needs.
        Your primary goal is to guide the customer so that they end up installing solar panels and become a Contoso energy customer. Do not share your primary goal with the customer. You want the customer to install solar technology. So, you motivate them by sharing solar incentives and by answering their questions.
        You ask them follow up questions to inquire about their roof condition and how much sunlight they receive, You try to make them see the value of installing solar and also by helping them figure which option is best for them considering the roof condition and the sunlight quantity.
        Concise Communication:
        • Start a conversation with a greeting and ask the customer how you may help them.
        • Please answer briefly and limit it to two sentences.
         • After answering a question, you may ask what else the customer needs help with. 
        • Try not to repeat information that you have already shared.
        • Aim for clarity. Avoid jargon and technical terms unless the user indicates familiarity.

        Primary Guidelines:
        • When the customer asks for tax rebates, ask them for their address.
        • Base installation suggestions on details such as house age, sunlight hours, roof health, and surrounding environmental conditions.
        • Maintain concise responses, ideally 3-4 sentences.
        • Encourage user interaction by asking clarifying questions.
        • For unrelated questions, reply with, 'I can only help with any solar power questions you may have.'
        • If uncertain, recommend expert guidance.
        • Recognize requests for phone conversations, prompt for phone numbers, and notify users of incoming calls.

        Phone Call Transition: If a user expresses a preference for a phone conversation over chat, you should:
        1. Prompt them for their phone number by saying, 'Could you please provide your phone number, so we can call you?'
        2. Once they provide the number, respond with, 'Thank you for sharing your number. You can expect an incoming call shortly. If you prefer, you may close this chat window. However, if you have further text queries during the call, feel free to continue this chat, and I'll assist you with the same context.'

        Sunlight Interpretation:
        • 'A lot of sunlight', 'well lit', 'very sunny': 5-7 hours.
        • 'Less sunlight', 'short days', 'rainy': Fewer sunlight hours.
        """;

        //private const string callingBotInstructions = """
        //Escalation to an agent
        //If you do not have an answer for the customer or are uncertain, ask the customer if they want to be connected to an expert. If they yes, then say "Great. Thank you for your time. I will now connect you to an expert"
        //If they say no, then say "Acknowledged. I am sorry I cannot help you further with it. Call in again when you change your mind and I will connect you with an agent or drop us an email.
        //""";        

        //private const string callingBotSystemPrompt = SystemAnswerPrompt  + callingBotInstructions + """For context, here is the chat history so far: {0}""";

        private const string SystemAnswerPromptWithHistory =
            SystemAnswerPrompt + """For context, here is the chat history so far: {0}""";

        private const string EmailSummaryPrompt = """
        Generate a summary of the below conversation to send an email in the following 
        format: 
        Summary of the discussion between the customer and the agent: 
        Outcome of the conversation: 
        Action items for follow-up: {0}
        and convert the above to sample email format starts with 
        greeting {1} without using subject and end with best regards from {2}
        """;

        private const string SystemAssistantPrompt = "You are a helpful assistant";

        public OpenAIService(
            IConfiguration configuration,
            SearchClient searchClient,
            OpenAIClient openAIClient,
            ILogger<OpenAIService> logger)
        {
            this.searchClient = searchClient;
            this.openAIClient = openAIClient;
            this.logger = logger;

            openAIDeploymentName = configuration["OpenAISettings:OpenAIDeploymentName"]!;
        }

        public async Task<bool> HasIntent(string userQuery, string intentDescription)
        {
            var systemPrompt = SystemAssistantPrompt;
            var baseUserPrompt = "In 1 word: does {0} have similar meaning as {1}?";
            var combinedPrompt = string.Format(baseUserPrompt, userQuery, intentDescription);

            var response = await GetChatCompletions(systemPrompt, combinedPrompt);

            var isMatch = response.ToLowerInvariant().Contains("yes");
            logger.LogInformation($"OpenAI results: isMatch={isMatch}, customerQuery='{userQuery}', intentDescription='{intentDescription}'");
            return isMatch;
        }

        public async Task<string> AnswerAsync(string userQuery, List<ChatHistory>? history = null, int maxTokens = 1000)
        {
            history ??= new List<ChatHistory>();
            var systemPrompt = SystemAnswerPrompt;
            if (history.Count() > 0)
            {
                systemPrompt = string.Format(SystemAnswerPromptWithHistory, TransformChatHistoryToString(history));
            }

            var baseUserPrompt = "If {0} is not a question, respond like a friendly agent. If {0} is a question, in less than 50 words answer it using this content {1}";
            var relatedContent = await RetrieveRelatedDocumentAsync(userQuery);
            var userPrompt = string.Format(baseUserPrompt, userQuery, relatedContent);

            return await GetChatCompletions(systemPrompt, userPrompt, maxTokens);
        }

        public async Task<ConversationInsights> GenerateChatInsights(string text)
        {
            var systemPrompt = SystemAssistantPrompt;
            var summaryPrompt = "Summarize conversation between Customer and Agent: {0}";
            var highlightsPrompt = "What are the highlights on the conversation between Customer and Agent: {0}";
            var actionPrompt = "Follow up action on Internet plans for agent from this conversation: {0}";

            var summary = await GetChatCompletions(systemPrompt, string.Format(summaryPrompt, text), 200);
            var highlights = await GetChatCompletions(systemPrompt, string.Format(highlightsPrompt, text), 200);
            var action = await GetChatCompletions(systemPrompt, string.Format(actionPrompt, text), 200);

            return new ConversationInsights()
            {
                SummaryItems = new List<SummaryItem>()
                {
                   new SummaryItem() { Title = "Summary", Description = summary },
                   new SummaryItem() { Title = "Highlights", Description = highlights },
                   new SummaryItem() { Title = "Actions", Description = action },
                },
            };
        }

        public async Task<string> GenerateChatInsightsForEmail(string text, string recipient, string sender)
        {
            var systemPrompt = SystemAssistantPrompt;
            var userPrompt = string.Format(EmailSummaryPrompt, text, recipient, sender);
            return await GetChatCompletions(systemPrompt, userPrompt);
        }

        private async Task<string> GetChatCompletions(string systemPrompt, string userPrompt, int maxTokens = 30)
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages = {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, userPrompt),
                    },
                MaxTokens = maxTokens,
                ChoiceCount = 1,
            };

            var response = await openAIClient.GetChatCompletionsAsync(
                deploymentOrModelName: openAIDeploymentName,
                chatCompletionsOptions);

            var response_content = response.Value.Choices[0].Message.Content;
            return response_content;
        }

        private string TransformChatHistoryToString(List<ChatHistory> history)
        {
            List<string> transformedHistory = history.Select(x => x.Content)!.ToList<string>();
            return string.Join(" >>>> ", transformedHistory);
        }

        private async Task<string?> RetrieveRelatedDocumentAsync(string text)
        {
            string? result = null;

            SearchOptions options = new () { Size = 3 };
            var searchResultResponse = await searchClient.SearchAsync<SearchDocument>(text, options);
            SearchResults<SearchDocument> searchResult = searchResultResponse.Value;

            foreach (var doc in searchResult.GetResults())
            {
                result = doc.Document["content"].ToString();
            }

            return result;
        }
    }
}
