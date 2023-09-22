// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Helpers
{
    public class ChatHelper
    {
        public static List<ChatHistory> GetChatHistoryWithThreadClient(ChatThreadClient chatThreadClient)
        {
            var chatMessages = chatThreadClient.GetMessages();
            List<ChatHistory> chatHistoryList = new();
            foreach (var chatMessage in chatMessages)
            {
                if (chatMessage.Sender?.RawId is not null)
                {
                    ChatHistory chatHistory = new()
                    {
                        MessageId = chatMessage.Id,
                        Content = chatMessage.Content?.Message,
                        SenderId = chatMessage.Sender?.RawId,
                        CreatedOn = chatMessage.CreatedOn,
                        MessageType = "chat",
                        ContentType = chatMessage.Type.ToString(),
                        SenderDisplayName = !string.IsNullOrEmpty(chatMessage.SenderDisplayName) ? chatMessage.SenderDisplayName : "Bot",
                    };
                    chatHistoryList.Add(chatHistory);
                }
            }

            return chatHistoryList;
        }
    }
}