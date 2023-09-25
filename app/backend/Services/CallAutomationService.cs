// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    public class CallAutomationService : ICallAutomationService
    {
        private readonly CallAutomationClient client;
        private readonly IConfiguration configuration;
        private readonly IIdentityService identityService;
        private readonly IMessageService messageService;
        private readonly IOpenAIService openAIService;
        private readonly ITranscriptionService transcriptionService;
        private readonly IJobRouterService jobRouterService;
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
            IMessageService messageService,
            IOpenAIService openAIService,
            ITranscriptionService transcriptionService,
            IJobRouterService jobRouterService,
            ILogger<CallAutomationService> logger)
        {
            this.cacheService = cacheService;
            this.configuration = configuration;
            this.identityService = identityService;
            this.messageService = messageService;
            this.openAIService = openAIService;
            this.transcriptionService = transcriptionService;
            this.jobRouterService = jobRouterService;
            this.logger = logger;

            var acsConnectionString = configuration["AcsConnectionString"];
            acsEndpoint = configuration["AcsEndpoint"] ?? "";
            cgsEndpoint = configuration["AzureAIServiceEndpoint"] ?? "";
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

                // Invoke transfer to agent
                if (configuration.GetValue<bool>("UseJobRouter") == true)
                {
                    // invoke transfer to agent via JobRouter
                    await AssignAgentViaJobRouter(recognizeCompleted.OperationContext!, targetParticipant);
                }
                else
                {
                    // Alternative: Connect directly /
                    await AssignAgentToCustomerDirectly(recognizeCompleted.OperationContext!, targetParticipant);
                }
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
                List<ChatHistory>? chatHistory = await GetFormattedChatHistory(threadId: recognizeCompleted.OperationContext!);

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

            // Do not retry on 401 errors
            if (resultInformation?.Code == 401)
            {
                await TranscribeBotVoice("Unable to establish call.");
                logger.LogError("Unable to use provided cognitive services resource. Please check the linking between communication and cognitive services resource");
                await callConnection.HangUpAsync(true);
                throw new ArgumentException(cgsEndpoint);
            }
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

        public async Task HandleEvent(PlayFailed playFailedEvent, string targetParticipant)
        {
            var callConnection = this.GetCallConnection(playFailedEvent.CallConnectionId);
            var callMedia = callConnection.GetCallMedia();
            var resultInformation = playFailedEvent.ResultInformation;
            logger.LogError("Encountered error during play, message={msg}, code={code}, subCode={subCode}", resultInformation?.Message, resultInformation?.Code, resultInformation?.SubCode);
            var reasonCode = playFailedEvent.ReasonCode;

            // In production, depending on the reason code (playFailedEvent.ReasonCode)  we could try to retry the play operation or do some more robust error handling to have a better customer experience
            await callConnection.HangUpAsync(true);
        }

        public async Task HandleEvent(CallEndedEvent callEndedEvent)
        {
            var groupId = callEndedEvent?.group?.id;
            var cachedGroupId = cacheService.GetCache("GroupId");

            if (!string.IsNullOrWhiteSpace(groupId) && groupId == cachedGroupId)
            {
                logger.LogInformation("Agent group call finished, groupCallId={}", groupId);

                if (configuration.GetValue<bool>("UseJobRouter"))
                {
                    /*
                    * If JobRouter is used to assign customer calls to available agents
                    * then it's also necessary to close the jobs once agent has finished handling the call
                    */
                    var job = await this.jobRouterService.GetJobAsync(groupId);
                    var assignmentId = job.Assignments.First().Key;

                    /* Complete the job */
                    await jobRouterService.CompleteJobAsync(job.Id, assignmentId);

                    /* Close the job */
                    await jobRouterService.CloseJobAsync(job.Id, assignmentId, dispositionCode: "Resolved");
                }
            }
        }

        public async Task ConnectAgentToCustomerAsync(string agentId, string threadId, string customerPhoneNumber)
        {
            // 1. Invite the agent to same chat thread; allow them to see past messages
            // As bot is the owner of the thread, it has permissions to add more participants
            var botUserId = cacheService.GetCache("BotUserId");
            var botToken = await identityService.GetTokenForUserId(botUserId);
            var chatClient = new ChatClient(
                endpoint: new Uri(acsEndpoint),
                communicationTokenCredential: new CommunicationTokenCredential(botToken));
            var chatThreadClient = chatClient.GetChatThreadClient(threadId: threadId);
            var chatParticipant = new ChatParticipant(new CommunicationUserIdentifier(agentId))
            {
                ShareHistoryTime = DateTimeOffset.UtcNow - TimeSpan.FromDays(1),
                DisplayName = "Technician"
            };
            await chatThreadClient.AddParticipantAsync(chatParticipant);

            // 2. Invite customer to join the call via SMS link
            var msgResp = await messageService.SendTextMessage($"{customerPhoneNumber.Trim()}".Trim());
            if (!msgResp.Successful)
            {
                logger.LogError("Failed to send text message to customer {phone}", customerPhoneNumber);
            }
        }

        private CallConnection GetCallConnection(string callConnectionId) =>
            client.GetCallConnection(callConnectionId);

        private async Task<bool> DetectEscalateToAgentIntent(string speechText) =>
            await openAIService.HasIntentAsync(userQuery: speechText, intentDescription: "talk to power company technician");

        private async Task<bool> DetectEndCallIntent(string speechText) =>
            await openAIService.HasIntentAsync(userQuery: speechText, intentDescription: "end call, hang up, end the call, goodbye");

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
            if (string.IsNullOrEmpty(threadId) || string.IsNullOrEmpty(botUserId))
            {
                return null;
            }
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

        private async Task AssignAgentToCustomerDirectly(string threadId, string targetParticipant)
        {
            var agentId = GetOrCreateAgentCommunicationUserId();
            cacheService.UpdateCache("GroupId", Guid.NewGuid().ToString());
            await ConnectAgentToCustomerAsync(agentId, threadId, targetParticipant);
        }

        private async Task AssignAgentViaJobRouter(string threadId, string targetParticipant)
        {
            var agentId = GetOrCreateAgentCommunicationUserId();
            var groupId = Guid.NewGuid().ToString();
            cacheService.UpdateCache("GroupId", groupId);
            await jobRouterService.CreateJobAsync(groupId, threadId, targetParticipant, agentId);
        }

        /*
        * This sample creates new AgentId on the fly and caches it to memcache.
        * Emulating a pool of agents with one agent.
        * Production scenario would include external directory of agents, including their assigned communication identitites
        */
        private string GetOrCreateAgentCommunicationUserId()
        {
            var agentId = cacheService.GetCache("AgentId");
            if (string.IsNullOrEmpty(agentId))
            {
                agentId = identityService.GetNewUserId();
                cacheService.UpdateCache("AgentId", agentId);
            }

            return agentId;
        }
    }
}