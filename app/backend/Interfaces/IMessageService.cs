// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IMessageService
    {
        Task<SmsSendResult> SendTextMessage(string callerId);
    }
}