// Copyright (c) Microsoft. All rights reserved.

namespace CustomerSupportServiceSample.Interfaces
{
    public interface ICacheService
    {
        string GetCache(string cacheKey);

        void UpdateCache(string cacheKey, string value);

        bool ClearCache();
    }
}
