// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Controllers
{
    [Route("api/conversation")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly ISummaryService summaryService;

        public SummaryController(ISummaryService summaryService)
        {
            this.summaryService = summaryService;
        }

        [HttpGet]
        [Route("insights/{threadId}")]
        public async Task<IActionResult> ConversationSummary([FromRoute] string threadId)
        {
            var response = await summaryService.GetConversationInsights(threadId);
            return Ok(response);
        }

        [HttpGet]
        [Route("emailSummary/{threadId}")]
        public async Task<ActionResult> SummaryForEmail(string threadId)
        {
            var summaryContent = await summaryService.GetEmailSummary(threadId);
            var response = new Dictionary<string, string>
            {
                { "result", summaryContent }
            };
            return Ok(response);
        }

        [HttpPost]
        [Route("emailSummary/sendEmail")]
        public async Task<ActionResult> SendSummaryEmail(SummaryRequest summaryRequest)
        {
            var resultStatus = await summaryService.SendSummaryEmail(summaryRequest);
            var response = new Dictionary<string, string>
            {
                { "result", resultStatus }
            };
            return Ok(response);
        }
    }
}