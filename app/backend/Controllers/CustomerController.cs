// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Controllers
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICacheService cacheService;
        private readonly IChatService chatService;

        public CustomerController(
            ICacheService cacheService,
            IChatService chatService)
        {
            this.cacheService = cacheService;
            this.chatService = chatService;
        }

        [HttpGet]
        [Route("info")]
        public IActionResult GetCustomerData()
        {
            var userId = cacheService.GetCache("UserId");
            var token = cacheService.GetCache("Token");
            var callId = cacheService.GetCache("GroupId");
            var response = new Dictionary<string, string>()
            {
                { "UserId", userId },
                { "Token", token },
                { "CallId", callId }
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("join/thread")]
        public async Task<ActionResult> GetOrCreateCustomerConversation()
        {
            var chatThreadResponse = await chatService.GetOrCreateCustomerConversation();
            return Ok(chatThreadResponse);
        }
    }
}
