// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface ISummaryService
    {
        Task<ConversationInsights> GetConversationInsights(string threadId);

        Task<string> GetEmailSummary(string threadId);

        Task<string> SendSummaryEmail(SummaryRequest summary);
    }
}