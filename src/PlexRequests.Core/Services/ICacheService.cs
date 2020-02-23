using System;
using System.Threading.Tasks;

namespace PlexRequests.Core.Services
{
    public interface ICacheService
    {
        Task<T> GetOrCreate<T>(string cacheKey, Func<Task<T>> createFunc);
        void Remove(string cacheKey);
    }
}
