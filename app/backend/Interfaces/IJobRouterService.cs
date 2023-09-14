// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Azure.Communication.JobRouter.Models;

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IJobRouterService
    {
        void SetUpRouter();

        Task<RouterJob> CreateJobAsync(string jobId, string threadId, string customerPhoneNumber, string agentId);

        RouterWorker CreateWorker(string agentId);

        Task<AcceptJobOfferResult> AcceptOfferAsync(string workerId, string offerId);

        Task CompleteJobAsync(string jobId, string assignmentId, string? note = null);

        Task CloseJobAsync(string jobId, string assignmentId, string? dispositionCode = null, string? note = null);

        Task<RouterJob> GetJobAsync(string jobId);
    }
}