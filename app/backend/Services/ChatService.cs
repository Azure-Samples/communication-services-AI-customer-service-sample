// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    public class ChatService : IChatService
    {
        private readonly IOpenAIService openAIService;
        private readonly IIdentityService identityService;
        private readonly ICacheService cacheService;
        private readonly ICallAutomationService callAutomationService;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly string acsEndpoint;
        private readonly string acsOutboundCallerId;
        private readonly string botUserId;
        private const string PSTNRegex = @"(\+\d{1,3}[-.\s]??\d{10}|\d{3}[-.\s]??\d{3}[-.\s]??\d{4}|\(\d{3}\)[-.\s]??\d{3}[-.\s]??\d{4})";

        public ChatService(
            IOpenAIService openAIService,
            IIdentityService identityService,
            ICacheService cacheService,
            ICallAutomationService callAutomationService,
            IConfiguration configuration,
            ILogger<ChatService> logger)
        {
            this.openAIService = openAIService;
            this.identityService = identityService;
            this.cacheService = cacheService;
            this.callAutomationService = callAutomationService;
            this.configuration = configuration;
            this.logger = logger;
            this.acsEndpoint = this.configuration["AcsSettings:AcsEndpoint"] ?? "";
            this.acsOutboundCallerId = this.configuration["AcsSettings:AcsPhoneNumber"] ?? "";
            ArgumentException.ThrowIfNullOrEmpty(acsEndpoint);
            ArgumentException.ThrowIfNullOrEmpty(acsOutboundCallerId);

            botUserId = identityService.GetNewUserId();
            cacheService.UpdateCache("BotUserId", botUserId);
        }

        public async Task<ChatClientResponse> GetOrCreateCustomerConversation()
        {
            var userId = cacheService.GetCache("UserId");
            var token = cacheService.GetCache("Token");
            var threadId = cacheService.GetCache("ThreadId");

            // 1. Create and cache new identity for customer if needed
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                (userId, token) = await identityService.GetNewUserIdAndToken();
                cacheService.UpdateCache("UserId", userId);
                cacheService.UpdateCache("Token", token);
            }

            // 2. Prepare new chat conversation as bot
            (var chatThreadClient, threadId) = await GetOrCreateBotChatThreadClient(threadId ?? null);
            cacheService.UpdateCache("ThreadId", threadId);

            // 3. Invite customer to conversation with bot
            chatThreadClient.AddParticipant(new ChatParticipant(new CommunicationUserIdentifier(userId))
            {
                DisplayName = "Customer"
            });

            return new ChatClientResponse
            {
                ThreadId = threadId,
                Token = token,
                Identity = userId,
                EndpointUrl = acsEndpoint,
                BotUserId = botUserId,
            };
        }

        public async Task HandleEvent(AcsChatMessageReceivedInThreadEventData chatEvent)
        {
            var eventSender = chatEvent.SenderCommunicationIdentifier.RawId;
            var eventMessage = chatEvent.MessageBody;
            var eventThreadId = chatEvent.ThreadId;
            var eventSenderType = chatEvent.Metadata.GetValueOrDefault("SenderType");

            if (eventThreadId != cacheService.GetCache("ThreadId"))
            {
                return; // only respond to active thread
            }

            if (eventSender == cacheService.GetCache("BotUserId"))
            {
                return; // don't respond to bot own messages
            }

            if (eventSenderType != null && eventSenderType.Equals("call", StringComparison.OrdinalIgnoreCase))
            {
                return; // don't respond to call transcript messages
            }

            if (eventSenderType != null && eventSenderType.Equals("bot", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            (var chatThreadClient, _) = await GetOrCreateBotChatThreadClient(eventThreadId);

            // 1. Handle handoff to voice call
            // Currently handoff is detected if message body contains a phone number
            // For more accurate results, it could be sent to OpenAI for analysis
            if (TryGetPhoneNumber(eventMessage, out var phoneNumber))
            {
                var sendChatMessageOptions = new SendChatMessageOptions()
                {
                    Content = "Thank you, I'm calling you now, and you can close this chat if you'd like.",
                    MessageType = ChatMessageType.Text
                };
                sendChatMessageOptions.Metadata.Add("SenderType", "bot");

                await chatThreadClient.SendMessageAsync(sendChatMessageOptions);
                await InitiateCallFromBot(phoneNumber, eventThreadId);
            }
            // 2. Respond with openAI generated response
            else
            {
                var chatGptResponse = await openAIService.AnswerAsync(eventMessage, GetFormattedChatHistory(chatThreadClient));
                var sendChatMessageOptions = new SendChatMessageOptions()
                {
                    Content = chatGptResponse,
                    MessageType = ChatMessageType.Text
                };
                sendChatMessageOptions.Metadata.Add("SenderType", "bot");
                await chatThreadClient.SendMessageAsync(chatGptResponse, ChatMessageType.Text);
            }
        }

        public async Task<List<ChatHistory>> GetChatHistory(string threadId)
        {
            (var chatThreadClient, _) = await GetOrCreateBotChatThreadClient(threadId);
            return GetFormattedChatHistory(chatThreadClient);
        }

        private List<ChatHistory> GetFormattedChatHistory(ChatThreadClient chatThreadClient)
        {
            List<ChatHistory> chatHistoryList = ChatHelper.GetChatHistoryWithThreadClient(chatThreadClient);
            return chatHistoryList.OrderBy(x => x.CreatedOn).ToList();
        }

        private async Task<(ChatThreadClient, string)> GetOrCreateBotChatThreadClient(string? threadId = null)
        {
            var botToken = await identityService.GetTokenForUserId(botUserId);
            ChatClient chatClient = new ChatClient(new Uri(acsEndpoint), new CommunicationTokenCredential(botToken));

            string? chatThreadId = threadId;

            if (string.IsNullOrEmpty(chatThreadId))
            {
                var botParticipant = new ChatParticipant(new CommunicationUserIdentifier(id: botUserId))
                {
                    DisplayName = "Bot"
                };
                CreateChatThreadResult createChatThreadResult = await chatClient.CreateChatThreadAsync(
                    topic: "Customer Support",
                    new[] { botParticipant });
                chatThreadId = createChatThreadResult.ChatThread.Id;
            }

            return (chatClient.GetChatThreadClient(chatThreadId), chatThreadId);
        }

        private static bool TryGetPhoneNumber(string message, out string phoneNumber)
        {
            Regex regex = new (PSTNRegex);
            MatchCollection matches = regex.Matches(message);
            if (matches.Count > 0)
            {
                phoneNumber = matches[0].Value;
                if (!phoneNumber.StartsWith("+"))
                {
                    phoneNumber = $"+{phoneNumber}";
                }
                phoneNumber = phoneNumber.Replace(" ", "");
                phoneNumber = phoneNumber.Replace("-", "");
                return true;
            }
            phoneNumber = "";
            return false;
        }

        private async Task InitiateCallFromBot(string phoneNumber, string threadId) =>
            await callAutomationService.CreateCallAsync(acsOutboundCallerId, phoneNumber, threadId);

    }
}