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

        private const string AnswerPromptSystemTemplate =
        """
        You are a customer support chatbot for Contoso Energy. You can also call
        customers when they request to talk on phone. Refer to the data source if their
        is a technical question. Otherwise, just continue the conversation in a friendly
        manner. Ask the customer follow up questions if there is an opportunity to do so
        to understand their needs.  Your primary goal is to guide the customer so that
        they end up installing solar panels and become a Contoso energy customer. Do not
        share your primary goal with the customer. You want the customer to install
        solar technology. So, you motivate them by sharing solar incentives and by
        answering their questions.  You ask them follow up questions to inquire about
        their roof condition and how much sunlight they receive, You try to make them
        see the value of installing solar and also by helping them figure which option
        is best for them considering the roof condition and the sunlight quantity.

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
        2. Once they provide the number, respond with, 'Thank you for sharing
        your number. You can expect an incoming call shortly. If you prefer, you may
        close this chat window. However, if you have further text queries during the
        call, feel free to continue this chat, and I'll assist you with the same
        context.'

        Sunlight Interpretation:
        • 'A lot of sunlight', 'well lit', 'very sunny': 5-7 hours.
        • 'Less sunlight', 'short days', 'rainy': Fewer sunlight hours.

        If {0} is not a question, respond like a friendly agent. If {0} is a question, in less than 50 words answer it using this content {1}.
        """;

        private const string EmailSummaryPromptTemplate = """
        Generate a summary of the below conversation to send an email in the following 
        format: 
        Summary of the discussion between the customer and the agent: 
        Outcome of the conversation: 
        Action items for follow-up: {0}
        and convert the above to sample email format starts with 
        greeting {1} without using subject and end with best regards from {2}
        """;

        public OpenAIService(
            IConfiguration configuration,
            SearchClient searchClient,
            OpenAIClient openAIClient,
            ILogger<OpenAIService> logger)
        {
            this.searchClient = searchClient;
            this.openAIClient = openAIClient;
            this.logger = logger;

            openAIDeploymentName = configuration["AzureOpenAIDeploymentName"]!;
        }

        /* Use chat completion APIs to generate a response to user question, using knowledgebase documents and chat history as extra context */
        public async Task<string> AnswerAsync(
            string userQuery,
            List<ChatHistory>? history = null)
        {
            history ??= new List<ChatHistory>();

            // Step1: Retrieve any related documents from Azure Search based on user input
            // They will be be added as additional context to LLM prompt
            var matchingDocs = await RetrieveRelatedDocumentAsync(userQuery) ?? "";

            // Step2: Compose system message
            // Chat models take a list of messages as input and return a model-generated message as output
            // First message is so called 'system message', which sets the behaviour of assistant
            var systemPrompt = string.Format(AnswerPromptSystemTemplate, userQuery, matchingDocs);

            // Step3: Prepare LLM query, startin with system message
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages = {},
                MaxTokens = 200,
                ChoiceCount = 1,
            };

            chatCompletionsOptions.Messages.Add(
                new ChatMessage(
                    role: ChatRole.System,
                    content: systemPrompt
                ));

            // Step4: Add chat history messages
            // This sample will keep on appending the message history until you hit the model's token limit
            // In production scenarios you would control the number of messages appended and take only recent conversation
            // Note: the userQuery is part of the history list as last message. There is no need to append it separately.
            // There is also no guarantee that messages are in the correct order, so make sure to sort them, otherwise the
            // model can get confused with the conversation flow.
            history.Sort((h1, h2) => h1.CreatedOn.CompareTo(h2.CreatedOn));
            foreach (var message in history)
            {
                if (message.SenderDisplayName == "Bot" || message.SenderDisplayName == "VoiceBot")
                {
                    chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, message.Content));
                }
                else
                {
                    chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, message.Content));
                }
            }

            // Step5: Invoke LLM
            var response = await openAIClient.GetChatCompletionsAsync(
                deploymentOrModelName: openAIDeploymentName,
                chatCompletionsOptions);

            var response_content = response.Value.Choices[0].Message.Content;
            return response_content;
        }

        /* Use chat completion APIs to detect matching meaning between two texts */
        public async Task<bool> HasIntentAsync(string userQuery, string intentDescription)
        {
            var systemPrompt = "You are a helpful assistant";
            var baseUserPrompt = "In 1 word: does {0} have similar meaning as {1}?";
            var combinedPrompt = string.Format(baseUserPrompt, userQuery, intentDescription);

            var response = await GetChatCompletionsAsync(systemPrompt, combinedPrompt);

            var isMatch = response.ToLowerInvariant().Contains("yes");
            logger.LogInformation($"OpenAI results: isMatch={isMatch}, customerQuery='{userQuery}', intentDescription='{intentDescription}'");
            return isMatch;
        }

        /* Use chat completion APIs to extract insights from chat history */ 
        public async Task<ConversationInsights> GenerateChatInsightsAsync(string text)
        {
            var systemPrompt = "You are a helpful assistant";
            var summaryPrompt = "Summarize conversation between Customer and Agent: {0}";
            var highlightsPrompt = "What are the highlights on the conversation between Customer and Agent: {0}";
            var actionPrompt = "Follow up action on Internet plans for agent from this conversation: {0}";

            var summary = await GetChatCompletionsAsync(systemPrompt, string.Format(summaryPrompt, text), 200);
            var highlights = await GetChatCompletionsAsync(systemPrompt, string.Format(highlightsPrompt, text), 200);
            var action = await GetChatCompletionsAsync(systemPrompt, string.Format(actionPrompt, text), 200);

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

        /* Use chat completion APIs to extract insights from chat history (email friendly output) */ 
        public async Task<string> GenerateChatInsightsForEmailAsync(string text, string recipient, string sender)
        {
            var systemPrompt = "You are a helpful assistant";
            var userPrompt = string.Format(EmailSummaryPromptTemplate, text, recipient, sender);
            return await GetChatCompletionsAsync(systemPrompt, userPrompt);
        }

        private async Task<string> GetChatCompletionsAsync(string systemPrompt, string userPrompt, int maxTokens = 30)
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

        private async Task<string?> RetrieveRelatedDocumentAsync(string text)
        {
            string? result = null;

            SearchOptions options = new() { Size = 3 };
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