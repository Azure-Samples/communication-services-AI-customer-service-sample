// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IMessageService
    {
        Task<SmsSendResult> SendTextMessage(string callerId);
    }
}