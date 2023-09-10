// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Services
{
    public class CallAutomationService : ICallAutomationService
    {
        private readonly CallAutomationClient client;
        private readonly IConfiguration configuration;
        private readonly IOpenAIService openAIService;
        private readonly IIdentityService identityService;
        private readonly ITranscriptionService transcriptionService;
        private readonly ICacheService cacheService;
        private readonly ILogger logger;

        private readonly string acsEndpoint;
        private readonly string cgsEndpoint;
        private readonly string baseUrl;
        private const string SpeechToTextVoice = "en-US-NancyNeural";
        private const string Greeting = "Hello, thank you for joining the call. I understand you're looking to add solar to your property";
        private const string EndCallPhraseToConnectAgent = "Sure, will send you the sms link to start the call with Power Company Technician. Thank you for calling, goodbye.";
        private const string EndCall = "Thank you for calling, goodbye.";
        private const string CustomerQueryTimeout = "I'm sorry, I wasn't quite able to hear that.";
        private const string NoResponse = "I didn't hear any input from you. Thank you, Goodbye.";
        private const string InvalidAudio = "Invalid speech phrase or tone detected, Disconnecting the call. Thank you!";

        public CallAutomationService(
            ICacheService cacheService,
            IConfiguration configuration,
            IIdentityService identityService,
            IOpenAIService openAIService,
            ITranscriptionService transcriptionService,
            ILogger<CallAutomationService> logger)
        {
            this.cacheService = cacheService;
            this.configuration = configuration;
            this.identityService = identityService;
            this.openAIService = openAIService;
            this.transcriptionService = transcriptionService;
            this.logger = logger;

            var acsConnectionString = configuration["AcsSettings:AcsConnectionString"];
            acsEndpoint = configuration["AcsSettings:AcsEndpoint"] ?? "";
            cgsEndpoint = configuration["CognitiveServiceSettings:CognitiveServiceEndpoint"] ?? "";
            baseUrl = configuration["HostUrl"] ?? "";
            ArgumentException.ThrowIfNullOrEmpty(acsConnectionString);
            ArgumentException.ThrowIfNullOrEmpty(acsEndpoint);
            ArgumentException.ThrowIfNullOrEmpty(cgsEndpoint);
            ArgumentException.ThrowIfNullOrEmpty(baseUrl);

            this.client = new CallAutomationClient(acsConnectionString);
        }

        public async Task<CreateCallResult> CreateCallAsync(string callerId, string targetParticipant, string threadId = "")
        {
            try
            {
                var callbackUri = new Uri(
                    baseUri: new Uri(baseUrl),
                    relativeUri: "/api/callbacks" + $"?targetParticipant={targetParticipant}");
                var target = new PhoneNumberIdentifier(targetParticipant);
                var caller = new PhoneNumberIdentifier(callerId);
                var callInvite = new CallInvite(target, caller);
                var createCallOptions = new CreateCallOptions(callInvite, callbackUri)
                {
                    CognitiveServicesEndpoint = new Uri(cgsEndpoint),
                    OperationContext = threadId
                };

                return await client.CreateCallAsync(createCallOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Could not create outbound call");
                throw;
            }
        }

        public async Task HandleEvent(CallConnected callConnected, string targetParticipant)
        {
            // Play a greeting and start listening for customer input
            var callMedia = GetCallConnection(callConnected.CallConnectionId).GetCallMedia();
            var recognizeOptions = GetMediaRecognizeSpeechOptions(Greeting, targetParticipant, callConnected.OperationContext!);
            await callMedia.StartRecognizingAsync(recognizeOptions);
            await TranscribeBotVoice(Greeting);
        }

        public async Task HandleEvent(PlayCompleted playCompleted, string targetParticipant)
        {
            // Play events in this sample only happen for final goodbye message. Once done, hang up the call
            var callConnection = GetCallConnection(playCompleted.CallConnectionId);
            await callConnection.HangUpAsync(true);
        }

        public async Task HandleEvent(RecognizeCompleted recognizeCompleted, string targetParticipant)
        {
            var callMedia = GetCallConnection(recognizeCompleted.CallConnectionId).GetCallMedia();
            var speech_result = (recognizeCompleted.RecognizeResult as SpeechResult)?.Speech ?? "";

            // 1. Customer and bot voice messages are logged down as transcript to chat history
            await TranscribeCustomerVoice(speech_result);

            // 2. Detect if customer is asking for agent
            if (await DetectEscalateToAgentIntent(speech_result))
            {
                var goodbye = EndCallPhraseToConnectAgent;
                var playOptions = GetPlaySpeechOptions(goodbye, targetParticipant);
                await callMedia.PlayAsync(playOptions);
                await TranscribeBotVoice(goodbye);
                // TODO: invoke transfer to agent via JobRouter
            }
            // 3. Detect if customer is ready to end call
            else if (await DetectEndCallIntent(speech_result))
            {
                var goodbye = EndCall;
                var playOptions = GetPlaySpeechOptions(goodbye, targetParticipant);
                await callMedia.PlayAsync(playOptions);
                await TranscribeBotVoice(goodbye);
            }
            // 4. Respond with openAI generated response
            else
            {
                // 5. For better OpenAI results, include complete conversation history to query
                // Using the original chat thread ID that was stored to OperationContext
                List<ChatHistory> chatHistory = await GetFormattedChatHistory(threadId: recognizeCompleted.OperationContext!);

                var chatGPTResponse = await openAIService.AnswerAsync(speech_result, chatHistory);

                var recognizeOptions = GetMediaRecognizeSpeechOptions(
                    chatGPTResponse,
                    targetParticipant,
                    recognizeCompleted.OperationContext);
                await callMedia.StartRecognizingAsync(recognizeOptions);
                await TranscribeBotVoice(chatGPTResponse);
            }
        }

        public async Task HandleEvent(RecognizeFailed recognizeFailedEvent, string targetParticipant)
        {
            var callConnection = this.GetCallConnection(recognizeFailedEvent.CallConnectionId);
            var callMedia = callConnection.GetCallMedia();
            var resultInformation = recognizeFailedEvent.ResultInformation;
            logger.LogError("Encountered error during recognize, message={msg}, code={code}, subCode={subCode}", resultInformation?.Message, resultInformation?.Code, resultInformation?.SubCode);
            var reasonCode = recognizeFailedEvent.ReasonCode;
            string replyText = reasonCode switch
            {
                var _ when reasonCode.Equals(MediaEventReasonCode.RecognizePlayPromptFailed) => CustomerQueryTimeout,
                var _ when reasonCode.Equals(MediaEventReasonCode.RecognizeInitialSilenceTimedOut) => NoResponse,
                var _ when reasonCode.Equals(MediaEventReasonCode.RecognizeIncorrectToneDetected) => InvalidAudio,
                _ => CustomerQueryTimeout,
            };
            var recognizeOptions = GetMediaRecognizeSpeechOptions(replyText, targetParticipant, recognizeFailedEvent.OperationContext!);
            await callMedia.StartRecognizingAsync(recognizeOptions);
            await TranscribeBotVoice(replyText);
        }

        private CallConnection GetCallConnection(string callConnectionId) =>
            client.GetCallConnection(callConnectionId);

        private async Task<bool> DetectEscalateToAgentIntent(string speechText) =>
            await openAIService.HasIntent(userQuery: speechText, intentDescription: "talk to agent, talk to technician, talk to company representative, talk to human");

        private async Task<bool> DetectEndCallIntent(string speechText) =>
            await openAIService.HasIntent(userQuery: speechText, intentDescription: "end call, hang up, end the call, goodbye");

        private async Task TranscribeCustomerVoice(string text) =>
            await transcriptionService.TranscribeVoiceMessageToChat(
                userId: cacheService.GetCache("UserId"),
                threadId: cacheService.GetCache("ThreadId"),
                displayName: "Customer (voice)",
                text: text);

        private async Task TranscribeBotVoice(string text) =>
            await transcriptionService.TranscribeVoiceMessageToChat(
                userId: cacheService.GetCache("BotUserId"),
                threadId: cacheService.GetCache("ThreadId"),
                displayName: "VoiceBot",
                text: text);

        private async Task<List<ChatHistory>> GetFormattedChatHistory(string threadId)
        {
            var botUserId = cacheService.GetCache("BotUserId");
            var botToken = await identityService.GetTokenForUserId(botUserId);

            var chatClient = new ChatClient(
                endpoint: new Uri(acsEndpoint),
                communicationTokenCredential: new CommunicationTokenCredential(botToken));

            var chatThreadClient = chatClient.GetChatThreadClient(threadId: threadId);

            var chatHistory = ChatHelper.GetChatHistoryWithThreadClient(chatThreadClient);
            return chatHistory;
        }

        private static CallMediaRecognizeSpeechOptions GetMediaRecognizeSpeechOptions(string content, string targetParticipant, string? threadId)
        {
            var playSource = new TextSource(content) { VoiceName = SpeechToTextVoice };

            var recognizeOptions =
                new CallMediaRecognizeSpeechOptions(targetParticipant: new PhoneNumberIdentifier(targetParticipant))
                {
                    InterruptCallMediaOperation = false,
                    InterruptPrompt = true,
                    InitialSilenceTimeout = TimeSpan.FromSeconds(20),
                    Prompt = playSource,
                    OperationContext = threadId,
                    EndSilenceTimeout = TimeSpan.FromMilliseconds(500),
                };

            return recognizeOptions;
        }

        private static PlayOptions GetPlaySpeechOptions(string content, string targetParticipant)
        {
            return new PlayOptions(
                playSource: new TextSource(content) { VoiceName = SpeechToTextVoice },
                playTo: new[] { new PhoneNumberIdentifier(targetParticipant) }); 
        }
    }
}
