// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    public class TranscriptionService : ITranscriptionService
    {
        private readonly IIdentityService identityService;
        private readonly string acsEndpoint;

        public TranscriptionService(
            IConfiguration configuration,
            IIdentityService identityService)
        {
            this.identityService = identityService;
            acsEndpoint = configuration["AcsSettings:AcsEndpoint"] ?? "";
            ArgumentException.ThrowIfNullOrEmpty(acsEndpoint);
        }

        public async Task TranscribeVoiceMessageToChat(
            string userId,
            string threadId,
            string text,
            string displayName)
        {
            if (string.IsNullOrEmpty(threadId) || string.IsNullOrEmpty(userId))
            {
                return;
            }
            // Retrieve user access token to be able to post the message on behalf of user
            var userToken = await identityService.GetTokenForUserId(userId);

            // Initialize chat client for threadId
            var chatThreadClient =
                new ChatClient(
                    endpoint: new Uri(acsEndpoint),
                    communicationTokenCredential: new CommunicationTokenCredential(userToken))
                .GetChatThreadClient(threadId: threadId);

            // Post message
            var sendMessagingOptions = new SendChatMessageOptions()
            {
                Content = text,
                MessageType = ChatMessageType.Text,
                SenderDisplayName = displayName,
            };

            // Metadata helps to filter these messages if needed
            sendMessagingOptions.Metadata.Add("SenderType", "Call");
            await chatThreadClient.SendMessageAsync(sendMessagingOptions);
        }
    }
}