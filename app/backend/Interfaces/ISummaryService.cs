// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface ISummaryService
    {
        Task<ConversationInsights> GetConversationInsights(string threadId);

        Task<string> GetEmailSummary(string threadId);
                    
        Task<string> SendSummaryEmail(SummaryRequest summary);
    }
}
