// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IOpenAIService
    {
        /* Returns true if user query matches the given intent */
        Task<bool> HasIntent(string userQuery, string intentDescription);

        /* Generates a response to user query */
        Task<string> AnswerAsync(string userQuery, List<ChatHistory>? history = null, int maxTokens = 1000);

        /* Generates summary, highlights and actions from chat conversation */
        Task<ConversationInsights> GenerateChatInsights(string text);

        /* Generates an email-ready summary from chat conversation */
        Task<string> GenerateChatInsightsForEmail(string text, string recipient, string sender);
    }
}