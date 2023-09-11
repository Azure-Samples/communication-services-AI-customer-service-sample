// Copyright (c) Microsoft. All rights reserved.

using System.Text;

namespace CustomerSupportServiceSample.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly IChatService chatService;
        private readonly IOpenAIService openAIService;
        private readonly ILogger logger;

        public SummaryService(
            IChatService chatService,
            IOpenAIService openAIService,
            ILogger<SummaryService> logger)
        {
            this.chatService = chatService;
            this.openAIService = openAIService;
            this.logger = logger;
        }

        public async Task<ConversationInsights> GetConversationInsights(string threadId)
        {
            var messages = await GetCombinedConversationFromThread(threadId);
            var response = await openAIService.GenerateChatInsights(messages);
            if (response != null)
            {
                response.ThreadId = threadId;
                return response;
            }
            return new ConversationInsights();
        }

        public async Task<string> GetEmailSummary(string threadId)
        {
            var messages = await GetCombinedConversationFromThread(threadId);
            return await openAIService.GenerateChatInsightsForEmail(messages, "Alice", "Agent");
        }

        public Task<string> SendSummaryEmail(SummaryRequest summary)
        {
            throw new NotImplementedException();
        }

        private async Task<string> GetCombinedConversationFromThread(string threadId, string optRecordingPath = "")
        {
            StringBuilder sbFinalConversation = new StringBuilder();
            string chatVoiceConvrsn = string.Empty;
            string voipConvrsn = string.Empty;
            try
            {
                chatVoiceConvrsn = await GetChatHistoryFromThreadId(threadId);
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception while Combining chat conversation from thread : {ex.Message}");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(optRecordingPath))
                {
                    voipConvrsn = await GetVoipHistoryFromThreadId(threadId, optRecordingPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception while Combining voip conversation from thread : {ex.Message}");
            }

            if (!string.IsNullOrWhiteSpace(chatVoiceConvrsn))
            {
                sbFinalConversation.AppendLine(chatVoiceConvrsn);
            }

            if (!string.IsNullOrWhiteSpace(voipConvrsn))
            {
                sbFinalConversation.AppendLine(voipConvrsn);
            }

            return sbFinalConversation.ToString();
        }

        private async Task<string> GetChatHistoryFromThreadId(string threadId)
        {
            var chatHistory = await chatService.GetChatHistory(threadId);

            var history = new List<IDictionary<string, string>>();
            foreach (ChatHistory chat in chatHistory)
            {
                var log = new Dictionary<string, string>
                {
                    { "speaker", string.IsNullOrEmpty(chat.SenderDisplayName) ? string.Empty : chat.SenderDisplayName },
                    { "text", string.IsNullOrEmpty(chat.Content) ? string.Empty : chat.Content },
                };
                history.Add(log);
            }

            StringBuilder sbConversation = new StringBuilder();
            foreach (var convItem in history)
            {
                string speakerName = convItem["speaker"];
                sbConversation.AppendLine($"{speakerName}:{convItem["text"]}");
            }

            return sbConversation.ToString();
        }

        private Task<string> GetVoipHistoryFromThreadId(string threadId, string recordFilePath = "")
        {
            // TODO: retrieve voip call transcript
            return Task.FromResult(string.Empty);
        }
    }
}
