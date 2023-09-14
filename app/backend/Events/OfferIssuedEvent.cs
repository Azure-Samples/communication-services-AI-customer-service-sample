// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Events
{
    public class OfferIssuedEvent
    {
        public string WorkerId { get; set; }

        public string JobId { get; set; }

        public string ChannelReference { get; set; }

        public string ChannelId { get; set; }

        public string QueueId { get; set; }

        public string OfferId { get; set; }

        public DateTimeOffset OfferTimeUtc { get; set; }

        public DateTimeOffset ExpiryTimeUtc { get; set; }

        public int JobPriority { get; set; }

        [JsonConverter(typeof(PrimitiveDictionaryConverter))]
        public Dictionary<string, object>? JobLabels { get; set; }

        [JsonConverter(typeof(PrimitiveDictionaryConverter))]
        public Dictionary<string, object>? JobTags { get; set; }
    }
}