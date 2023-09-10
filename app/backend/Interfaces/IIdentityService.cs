// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IIdentityService
    {
        string GetNewUserId();

        Task<(string, string)> GetNewUserIdAndToken();

        Task<string> GetTokenForUserId(string userId);
    }
}
