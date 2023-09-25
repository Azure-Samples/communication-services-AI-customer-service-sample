// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly IChatService chatService;
        private readonly IOpenAIService openAIService;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly string acsConnectionString;
        private readonly string sender;
        private readonly string recipient;

        public SummaryService(
            IChatService chatService,
            IOpenAIService openAIService,
            IConfiguration configuration,
            ILogger<SummaryService> logger)
        {
            this.chatService = chatService;
            this.openAIService = openAIService;
            this.logger = logger;
            this.configuration = configuration;
            acsConnectionString = this.configuration["AcsConnectionString"] ?? "";
            sender = this.configuration["EmailSender"] ?? "";
            recipient = this.configuration["EmailRecipient"] ?? "";
            ArgumentException.ThrowIfNullOrEmpty(acsConnectionString);
        }

        public async Task<ConversationInsights> GetConversationInsights(string threadId)
        {
            var messages = await GetConversations(threadId);
            var response = await openAIService.GenerateChatInsightsAsync(messages);
            if (response != null)
            {
                response.ThreadId = threadId;
                return response;
            }
            return new ConversationInsights();
        }

        public async Task<string> GetEmailSummary(string threadId)
        {
            var messages = await this.GetConversations(threadId);
            return await this.openAIService.GenerateChatInsightsForEmailAsync(messages, "Alice", "Agent");
        }

        public async Task<string> SendSummaryEmail(SummaryRequest summary)
        {
            var htmlContent = summary.Body;
            try
            {
                logger.LogInformation("Sending email: to={}, from={}, body={}", recipient, sender, htmlContent);
                ArgumentException.ThrowIfNullOrEmpty(sender);
                ArgumentException.ThrowIfNullOrEmpty(recipient);
                // Note: 
                // This quickstart sample uses receiver email address from app configuration for simplicity
                // In production scenario customer would provide their preferred email address
                EmailClient emailClient = new(this.acsConnectionString);
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    WaitUntil.Completed,
                    sender,
                    recipient,
                    "Follow up on support conversation",
                    htmlContent);
                return emailSendOperation.Value.Status.ToString();
            }
            catch (RequestFailedException ex)
            {
                this.logger.LogError($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
                return ex.ErrorCode ?? "EmailSendFailed";
            }
        }

        private async Task<string> GetConversations(string threadId)
        {
            var conversationHistory = await chatService.GetChatHistory(threadId);
            StringBuilder sbConversation = new StringBuilder();
            foreach (var conversation in conversationHistory)
            {
                sbConversation.Append($"{conversation.SenderDisplayName}: {conversation.Content}");
                sbConversation.AppendLine();
            }

            return sbConversation.ToString();
        }
    }
}