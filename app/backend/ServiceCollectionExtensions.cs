// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services)
    {
        services.AddSingleton<SearchClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureSearchServiceEndpoint, azureSearchIndex, azureSearchKey) =
                (config["AzureSearchEndpoint"],
                 config["AzureSearchIndexName"],
                 config["AzureSearchKey"]);

            ArgumentException.ThrowIfNullOrEmpty(azureSearchServiceEndpoint);
            ArgumentException.ThrowIfNullOrEmpty(azureSearchKey);

            Azure.AzureKeyCredential credential = new(azureSearchKey);

            var searchClient = new SearchClient(
                new Uri(azureSearchServiceEndpoint), azureSearchIndex, credential);

            return searchClient;
        });

        services.AddSingleton<OpenAIClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureOpenAiServiceEndpoint, azureOpenAiKey) =
                (config["AzureOpenAIEndpoint"],
                config["AzureOpenAIKey"]);

            ArgumentException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);
            ArgumentException.ThrowIfNullOrEmpty(azureOpenAiKey);

            var openAIClient = new OpenAIClient(
                new Uri(azureOpenAiServiceEndpoint), new AzureKeyCredential(azureOpenAiKey));

            return openAIClient;
        });

        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ICallAutomationService, CallAutomationService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddSingleton<IJobRouterEventsService, JobRouterEventsService>();
        services.AddSingleton<IJobRouterService, JobRouterService>(js =>
        {
            var config = js.GetRequiredService<IConfiguration>();

            var identityService = js.GetRequiredService<IIdentityService>();
            var cacheService = js.GetRequiredService<ICacheService>();
            var logger = js.GetRequiredService<ILogger<JobRouterService>>();
            var jobService = new JobRouterService(config, logger);

            // set up the policies and queue
            jobService.SetUpRouter();

            // Identify agent who will handle the case
            var agentUserId = identityService.GetNewUserId();

            // update agent id in the cache
            cacheService.UpdateCache("AgentId", agentUserId);

            // create the worker
            jobService.CreateWorker(agentUserId);
            return jobService;
        });

        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IOpenAIService, OpenAIService>();
        services.AddSingleton<ISummaryService, SummaryService>();
        services.AddSingleton<ITranscriptionService, TranscriptionService>();
        return services;
    }
}