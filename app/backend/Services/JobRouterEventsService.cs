// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    public class JobRouterEventsService : IJobRouterEventsService
    {
        private readonly ILogger logger;
        private readonly ICallAutomationService callAutomationService;
        private readonly IJobRouterService jobRouterService;
        private readonly ICacheService cacheService;

        public JobRouterEventsService(ILogger<JobRouterEventsService> logger,
            ICallAutomationService callAutomationService,
            IJobRouterService jobRouterService,
            ICacheService cacheService)
        {
            this.logger = logger;
            this.callAutomationService = callAutomationService;
            this.jobRouterService = jobRouterService;
            this.cacheService = cacheService;
        }

        public async Task HandleEvent(OfferIssuedEvent offerIssuedEvent)
        {
            /*  accept the job offer */
            await jobRouterService.AcceptOfferAsync(offerIssuedEvent.WorkerId, offerIssuedEvent.OfferId);
        }

        public async Task HandleEvent(OfferAcceptedEvent offerAcceptedEvent)
        {
            try
            {
                // WorkerId encodes ':' to '__'. 
                // Decode it back to get correct agentId value
                var workerUserId = offerAcceptedEvent?.WorkerId ?? "";
                var agentUserId = workerUserId.Replace("__", ":");
                string threadId = offerAcceptedEvent?.JobTags?["threadId"]?.ToString() ?? string.Empty;
                string customerPhoneNumber = offerAcceptedEvent?.JobTags?["customerPhoneNumber"]?.ToString() ?? string.Empty;

                /* assign the agent to customer */
                await callAutomationService.ConnectAgentToCustomerAsync(agentUserId, threadId, customerPhoneNumber);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)System.Net.HttpStatusCode.NotFound)
            {
                /* call not found, complete the job */
                logger.LogError(ex, $"Handle OfferAcceptedEvent tried to join a call that was no longer active, call ID '{offerAcceptedEvent.JobId}'");
                await jobRouterService.CompleteJobAsync(offerAcceptedEvent.JobId, offerAcceptedEvent.AssignmentId);

                await jobRouterService.CloseJobAsync(offerAcceptedEvent.JobId, offerAcceptedEvent.AssignmentId);
            }
            catch (RequestFailedException ex)
            {
                logger.LogError(ex, "Handle OfferAcceptedEvent failed unexpectedly");
                throw;
            }

        }
    }
}