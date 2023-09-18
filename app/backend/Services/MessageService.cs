// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger logger;
        private readonly SmsClient smsClient;
        private readonly string smsMessage;
        private readonly string senderPhoneNumber;

        public MessageService(
            IConfiguration configuration,
            ILogger<MessageService> logger)
        {
            this.logger = logger;
            smsClient = new SmsClient(configuration["AcsSettings:AcsConnectionString"]);
            senderPhoneNumber = configuration["AcsSettings:AcsPhonenumber"]!;

            // Note: As this sample supports only one conversation at a time
            // there is no need to embed call identifier to url. So the URL is static
            var callJoinUrl = $"{configuration["HostUrl"]}?callerType=Customer";
            smsMessage = string.Format(
                "To start a call with the Power Company Technician, please click on this link: {0}",
                callJoinUrl);
        }

        public async Task<SmsSendResult> SendTextMessage(string targetPhoneNumber)
        {
            SmsSendResult resp = await smsClient.SendAsync(
                from: senderPhoneNumber, // Your E.164 formatted from phone number used to send SMS
                to: targetPhoneNumber, // E.164 formatted recipient phone number
                message: smsMessage);
            logger.LogInformation("Sent SMS message, to={target}, message={message}", targetPhoneNumber, smsMessage);
            return resp;
        }
    }
}