// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services)
    {
        services.AddSingleton<SearchClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureSearchServiceEndpoint, azureSearchIndex, azureSearchKey) =
                (config["CognitiveServiceSettings:CognitiveSearchEndpoint"],
                 config["CognitiveServiceSettings:IndexName"],
                 config["CognitiveServiceSettings:CognitiveSearchKey"]);

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
                (config["OpenAISettings:OpenAIEndpoint"],
                config["OpenAISettings:OpenAIKey"]);

            ArgumentException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);
            ArgumentException.ThrowIfNullOrEmpty(azureOpenAiKey);

            var openAIClient = new OpenAIClient(new Uri(azureOpenAiServiceEndpoint), new AzureKeyCredential(azureOpenAiKey));

            return openAIClient;
        });

        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ICallAutomationService, CallAutomationService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddSingleton<IOpenAIService, OpenAIService>();
        services.AddSingleton<ITranscriptionService, TranscriptionService>();
        return services;
    }
}
