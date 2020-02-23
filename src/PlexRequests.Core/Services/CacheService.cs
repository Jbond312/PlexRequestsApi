using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace PlexRequests.Core.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetOrCreate<T>(string cacheKey, Func<Task<T>> createFunc)
        {
            if (_memoryCache.TryGetValue(cacheKey, out T result))
            {
                return result;
            }

            result = await createFunc();

            _memoryCache.Set(cacheKey, result, DateTime.Now.AddHours(1));

            return result;
        }

        public void Remove(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }
    }
}
