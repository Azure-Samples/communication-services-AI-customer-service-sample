// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Models
{
    public class SummaryRequest
    {
        [JsonPropertyName("toEmail")]
        public string? ToEmail { get; set; }

        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }
}