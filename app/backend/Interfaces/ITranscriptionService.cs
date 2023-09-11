// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface ITranscriptionService
    {
        Task TranscribeVoiceMessageToChat(string userId, string threadId, string text, string displayName);
    }
}