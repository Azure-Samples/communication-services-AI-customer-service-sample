// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Models
{
    public class ChatClientResponse
    {
        [JsonPropertyName("threadId")]
        public string? ThreadId { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("identity")]
        public string? Identity { get; set; }

        [JsonPropertyName("endpointUrl")]
        public string? EndpointUrl { get; set; }

        [JsonPropertyName("messageId")]
        public string? MessageId { get; set; }

        [JsonPropertyName("botUserId")]
        public string? BotUserId { get; set; }
    }
}
