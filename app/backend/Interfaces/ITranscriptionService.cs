// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface ITranscriptionService
    {
        Task TranscribeVoiceMessageToChat(string userId, string threadId, string text, string displayName);
    }
}