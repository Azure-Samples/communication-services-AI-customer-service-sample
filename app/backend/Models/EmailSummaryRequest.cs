// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Models
{
    public class SummaryRequest
    {
        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }
}