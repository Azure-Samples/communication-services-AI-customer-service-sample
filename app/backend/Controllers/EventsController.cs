// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Controllers
{
    [Route("api")]
    [ApiController]
    public class EventsController : Controller
    {
        private readonly ICacheService cacheService;
        private readonly ICallAutomationService callAutomationService;
        private readonly IChatService chatService;
        private readonly ILogger logger;
        private readonly EventConverter eventConverter;


        public EventsController(
            ICacheService cacheService,
            ICallAutomationService callAutomationService,
            IChatService chatService,
            ILogger<EventsController> logger)
        {
            this.cacheService = cacheService;
            this.callAutomationService = callAutomationService;
            this.chatService = chatService;
            this.logger = logger;
            eventConverter = new EventConverter();
        }

        /* Route for Azure Communication Service eventgrid webhooks */
        [HttpPost]
        [Route("events")]
        public async Task<IActionResult> Handle([FromBody] EventGridEvent[] eventGridEvents)
        {
            foreach (var eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.TryGetSystemEventData(out object eventData))
                {
                    if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                    {
                        var responseData = new SubscriptionValidationResponse
                        {
                            ValidationResponse = subscriptionValidationEventData.ValidationCode
                        };

                        return Ok(responseData);
                    }
                }

                var data = eventConverter.Convert(eventGridEvent);
                switch (data)
                {
                    case null:
                        continue;

                    case AcsChatMessageReceivedInThreadEventData chatMessageReceived:
                        await chatService.HandleEvent(chatMessageReceived);
                        break;

                    case CallEndedEvent callEnded:
                        await callAutomationService.HandleEvent(callEnded);
                        break;
                }
            }

            return Ok();
        }

        /* Route for CallAutomation in-call event callbacks */
        [HttpPost]
        [Route("callbacks")]
        public async Task<IActionResult> Handle([FromBody] CloudEvent[] cloudEvents, [FromQuery(Name = "targetParticipant")] string targetParticipant)
        {
            foreach (var cloudEvent in cloudEvents)
            {
                CallAutomationEventBase parsedEvent = CallAutomationEventParser.Parse(cloudEvent);
                targetParticipant = $"+" + targetParticipant.Trim();
                logger.LogInformation(
                    "Received call event: {type}, callConnectionID: {connId}, serverCallId: {serverId}, chatThreadId: {chatThreadId}",
                    parsedEvent.GetType(),
                    parsedEvent.CallConnectionId,
                    parsedEvent.ServerCallId,
                    parsedEvent.OperationContext);

                switch (parsedEvent)
                {
                    case CallConnected callConnected:
                        await callAutomationService.HandleEvent(callConnected, targetParticipant);
                        break;

                    case RecognizeCompleted recognizeCompleted:
                        await callAutomationService.HandleEvent(recognizeCompleted, targetParticipant);
                        break;

                    case RecognizeFailed recognizeFailed:
                        await callAutomationService.HandleEvent(recognizeFailed, targetParticipant);
                        break;

                    case PlayCompleted playCompleted:
                        await callAutomationService.HandleEvent(playCompleted, targetParticipant);
                        break;

                    case PlayFailed playFailed:
                        await callAutomationService.HandleEvent(playFailed, targetParticipant);
                        break;

                    default:
                        break;
                }
            }

            return Ok();
        }
    }
}