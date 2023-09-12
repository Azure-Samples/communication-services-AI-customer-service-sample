// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface ICacheService
    {
        string GetCache(string cacheKey);

        void UpdateCache(string cacheKey, string value);

        bool ClearCache();
    }
}