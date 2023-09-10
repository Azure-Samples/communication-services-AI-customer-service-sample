// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Controllers
{
    [Route("api/debug")]
    [ApiController]
    /* These API routes are for developer debug purposes. Not used by sample webapp */
    public class DebugController : Controller
    {
        private readonly ICacheService cacheService;
        private readonly ICallAutomationService callAutomationService;
        private readonly IConfiguration configuration;

        public DebugController(
            ICacheService cacheService,
            ICallAutomationService callAutomationService,
            IConfiguration configuration)
        {
            this.cacheService = cacheService;
            this.callAutomationService = callAutomationService;
            this.configuration = configuration;
        }

        [HttpGet]
        [Route("clearCache")]
        public ActionResult ClearCache()
        {
            var result =  cacheService.ClearCache();
            return Ok(result);
        }

        [HttpPost]
        [Route("CallToPSTN")]
        public async Task<IActionResult> CreateCall(string targetPSTNNumber = "", string threadId = "")
        {
            string callerId = configuration["AcsSettings:AcsPhoneNumber"] ?? "";
            return Ok(await callAutomationService.CreateCallAsync(callerId, targetPSTNNumber, threadId));
        }
    }
}
