// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace CustomerSupportServiceSample.Services
{
    using CustomerSupportServiceSample.Interfaces;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public string GetCache(string cacheKey)
        {
            if (memoryCache.TryGetValue(cacheKey, out string? cacheValue) && cacheValue != null)
            {
                return (string)cacheValue;
            }
            return string.Empty;
        }

        public void SetCache(Dictionary<string, string> keyValuePairs)
        {
            foreach (var keyValue in keyValuePairs)
            {
                memoryCache.Set(keyValue.Key, keyValue.Value);
            }
        }

        public void UpdateCache(string cacheKey, string value)
        {
            memoryCache.Set(cacheKey, value);
        }

        public bool ClearCache()
        {
            memoryCache.Remove("Token");
            memoryCache.Remove("UserId");
            memoryCache.Remove("AgentId");
            memoryCache.Remove("BotUserId");
            memoryCache.Remove("ThreadId");
            return true;
        }
    }
}