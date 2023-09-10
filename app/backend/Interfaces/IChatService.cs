// Copyright (c) Microsoft. All rights reserved.

using ChatMessage = Azure.Communication.Chat.ChatMessage;

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IChatService
    {
        Task<ChatClientResponse> GetOrCreateCustomerConversation();

        Task<List<ChatHistory>> GetChatHistory(string threadId);

        Task HandleEvents(AcsChatMessageReceivedInThreadEventData eventGridEvent);
    }
}
