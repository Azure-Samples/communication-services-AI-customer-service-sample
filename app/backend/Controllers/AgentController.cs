// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Controllers
{
    [Route("api/agent")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly ICacheService cacheService;
        private readonly IConfiguration configuration;
        private readonly IChatService chatService;
        private readonly IIdentityService identityService;
        private readonly ILogger logger;

        public AgentController(
            ICacheService cacheService,
            IConfiguration configuration,
            IChatService chatService,
            IIdentityService identityService,
            ILogger<AgentController> logger)
        {
            this.cacheService = cacheService;
            this.configuration = configuration;
            this.chatService = chatService;
            this.identityService = identityService;
            this.logger = logger;
        }

        [HttpGet]
        [Route("info")]
        public async Task<IActionResult> GetAgentData()
        {
            var agentId = cacheService.GetCache("AgentId");
            var threadId = cacheService.GetCache("ThreadId");
            var callId = cacheService.GetCache("GroupId");
            var token = await identityService.GetTokenForUserId(agentId);
            var endPointUrl = configuration["AcsSettings:AcsEndpoint"] ?? "";

            var response = new Dictionary<string, string>()
            {
                { "AgentId", agentId },
                { "Token", token },
                { "CallId", callId },
                { "ThreadId", threadId },
                { "EndPointUrl", endPointUrl },
            };
            return Ok(response);
        }
    }
}
