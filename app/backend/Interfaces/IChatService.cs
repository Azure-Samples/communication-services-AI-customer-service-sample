// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IChatService
    {
        Task<ChatClientResponse> GetOrCreateCustomerConversation();

        Task<List<ChatHistory>> GetChatHistory(string threadId);

        Task HandleEvents(AcsChatMessageReceivedInThreadEventData eventGridEvent);
    }
}