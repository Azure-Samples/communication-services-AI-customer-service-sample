﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

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
        private readonly IMessageService messageService;

        public DebugController(
            ICacheService cacheService,
            ICallAutomationService callAutomationService,
            IMessageService messageService,
            IConfiguration configuration)
        {
            this.cacheService = cacheService;
            this.callAutomationService = callAutomationService;
            this.messageService = messageService;
            this.configuration = configuration;
        }

        [HttpGet]
        [Route("clearCache")]
        public ActionResult ClearCache()
        {
            var result = cacheService.ClearCache();
            return Ok(result);
        }

        [HttpPost]
        [Route("callToPstn")]
        public async Task<IActionResult> CreateCall(string targetPSTNNumber = "", string threadId = "")
        {
            string callerId = configuration["AcsPhoneNumber"] ?? "";
            return Ok(await callAutomationService.CreateCallAsync(callerId, targetPSTNNumber, threadId));
        }

        [HttpPost]
        [Route("sendSms")]
        public async Task<IActionResult> SendSms(string targetPSTNNumber = "")
        {
            return Ok(await messageService.SendTextMessage(targetPSTNNumber));
        }
    }
}