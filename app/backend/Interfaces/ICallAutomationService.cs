// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface ICallAutomationService
    {
        Task<CreateCallResult> CreateCallAsync(string callerId, string targetParticipant, string threadId = "");

        Task HandleEvent(CallConnected callConnected, string targetParticipant);

        Task HandleEvent(RecognizeCompleted recognizeCompleted, string targetParticipant);

        Task HandleEvent(RecognizeFailed recognizeFailedEvent, string targetParticipant);

        Task HandleEvent(PlayCompleted playCompletedEvent, string targetParticipant);

        Task HandleEvent(PlayFailed playFailedEvent, string targetParticipant);
    }
}