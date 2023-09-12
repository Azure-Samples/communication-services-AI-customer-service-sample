// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface IIdentityService
    {
        string GetNewUserId();

        Task<(string, string)> GetNewUserIdAndToken();

        Task<string> GetTokenForUserId(string userId);
    }
}