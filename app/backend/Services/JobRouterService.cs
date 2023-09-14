// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    public class JobRouterService : IJobRouterService
    {
        public static RouterQueue routerQueue;
        private readonly ILogger logger;
        private readonly JobRouterClient jobRouterClient;
        private readonly JobRouterAdministrationClient jobRouterAdministrationClient;
        private readonly string acsConnectionString;
        private const string QueueName = "AgentQueue";
        private const string QueueId = "AgentQueue_1";
        private const string PolicyId = "Policy1";
        private const string PolicyName = "AgentDistributionPolicy";

        public JobRouterService(IConfiguration configuration,
            ILogger<JobRouterService> logger)
        {
            this.logger = logger;
            this.acsConnectionString = configuration["AcsConnectionString"] ?? "";
            ArgumentException.ThrowIfNullOrEmpty(this.acsConnectionString);
            this.jobRouterClient = new JobRouterClient(this.acsConnectionString);
            this.jobRouterAdministrationClient = new JobRouterAdministrationClient(this.acsConnectionString);
            this.jobRouterAdministrationClient = new JobRouterAdministrationClient(this.acsConnectionString);
        }

        /* set up queue and policies for the job router */
        public void SetUpRouter()
        {
            var distributionPolicy = jobRouterAdministrationClient.CreateDistributionPolicy(
                new CreateDistributionPolicyOptions(
                    distributionPolicyId: PolicyId,
                    offerExpiresAfter: TimeSpan.FromMinutes(1),
                    mode: new LongestIdleMode())
                {
                    Name = PolicyName
                });

            routerQueue = jobRouterAdministrationClient.CreateQueue(
                 new CreateQueueOptions(queueId: QueueId, distributionPolicyId: distributionPolicy.Value.Id)
                 {
                     Name = QueueName
                 });
        }

        /* create worker */
        public RouterWorker CreateWorker(string agentId)
        {
            // WorkerId cannot contain ':', so it's encoded to '__'
            var workerId = agentId.Replace(":", "__");
            var worker = jobRouterClient.CreateWorker(new CreateWorkerOptions(workerId: workerId, totalCapacity: 100)
            {
                AvailableForOffers = true,
                QueueAssignments = { [routerQueue.Id] = new RouterQueueAssignment() },
                Labels = { ["agent"] = new LabelValue(agentId) },
                ChannelConfigurations = { ["voip"] = new ChannelConfiguration(capacityCostPerJob: 1) },
            });

            return worker;
        }

        /* create the job for the worker */
        public async Task<RouterJob> CreateJobAsync(string jobId, string threadId, string customerPhoneNumber, string agentId)
        {
            Response<RouterJob> job = await jobRouterClient.CreateJobAsync(new CreateJobOptions(jobId, "voip", QueueId)
            {
                Priority = 1,
                RequestedWorkerSelectors =
                {
                    new RouterWorkerSelector(key: "agent", labelOperator: LabelOperator.Equal, value: new LabelValue(agentId)),
                },
                Tags = { ["threadId"] = new LabelValue(threadId), ["customerPhoneNumber"] = new LabelValue(customerPhoneNumber) },
            });

            return job;
        }

        /* Accept the job offer */
        public async Task<AcceptJobOfferResult> AcceptOfferAsync(string workerId, string offerId)
        {
            var result = await jobRouterClient.AcceptJobOfferAsync(workerId, offerId);
            return result.Value;
        }

        /* Complete the assigned job */
        public async Task CompleteJobAsync(string jobId, string assignmentId, string? note = null)
        {
            await jobRouterClient.CompleteJobAsync(new CompleteJobOptions(jobId, assignmentId) { Note = note });
        }

        /* Close the assigned the job for the worker */
        public async Task CloseJobAsync(string jobId, string assignmentId, string? dispositionCode = null, string? note = null)
        {
            await jobRouterClient.CloseJobAsync(new CloseJobOptions(jobId, assignmentId)
            {
                DispositionCode = dispositionCode,
                Note = note
            });
        }

        /* Get the job for job id */
        public async Task<RouterJob> GetJobAsync(string jobId)
        {
            var job = await jobRouterClient.GetJobAsync(jobId);
            return job.Value;
        }

        private async Task CancelJobAsync(string jobId, string? dispositionCode = null, string? note = null)
        {
            await jobRouterClient.CancelJobAsync(new CancelJobOptions(jobId)
            {
                DispositionCode = dispositionCode,
                Note = note,
            });
        }
    }
}