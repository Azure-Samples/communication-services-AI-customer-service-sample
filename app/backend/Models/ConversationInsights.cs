// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Models
{
    public class ConversationInsights
    {
        public string? TopicName { get; set; }

        public string? ThreadId { get; set; }

        public List<SummaryItem>? SummaryItems { get; set; }
    }

    public class SummaryItem
    {
        public string? Title { get; set; }

        public string? Description { get; set; }
    }
}
    
