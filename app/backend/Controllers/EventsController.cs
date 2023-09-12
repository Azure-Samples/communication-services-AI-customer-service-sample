// Copyright (c) Microsoft. All rights reserved.

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
                    else if (eventData is AcsChatMessageReceivedInThreadEventData chatEventData)
                    {
                        await chatService.HandleEvents(chatEventData);
                    }
                }
                else if (eventGridEvent.EventType == "Microsoft.Communication.CallStarted")
                {
                    var callEventData = eventGridEvent.Data.ToObjectFromJson<CallStartedEventData>();
                    var groupId = callEventData?.group?.id;
                    var cachedGroupId = cacheService.GetCache("GroupId");

                    if (!string.IsNullOrEmpty(groupId) && groupId == cachedGroupId)
                    {
                        logger.LogInformation("Group call started, groupCallId={}", groupId);
                    }
                }
                else if (eventGridEvent.EventType == "Microsoft.Communication.CallEnded")
                {
                    var callEventData = eventGridEvent.Data.ToObjectFromJson<CallStartedEventData>();
                    var groupId = callEventData?.group?.id;
                    var cachedGroupId = cacheService.GetCache("GroupId");

                    if (!string.IsNullOrWhiteSpace(groupId) && groupId == cachedGroupId)
                    {
                        logger.LogInformation("Group call finished, groupCallId={}", groupId);
                    }
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
        private class CallStartedEventData
        {
            public string? serverCallId { get; set; }
            public Group? group { get; set; }
            public string? correlationId { get; set; }
        }
        private class Group
        {
            public string? id { get; set; }
        }
    }
}