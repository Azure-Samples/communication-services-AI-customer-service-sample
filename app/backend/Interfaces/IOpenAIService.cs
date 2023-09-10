// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IOpenAIService
    {
        /* Returns true if user query matches the given intent */
        Task<bool> HasIntent(string userQuery, string intentDescription);

        /* Generates a response to user query */
        Task<string> AnswerAsync(string userQuery, List<ChatHistory>? history = null, int maxTokens = 1000);
    }
}
