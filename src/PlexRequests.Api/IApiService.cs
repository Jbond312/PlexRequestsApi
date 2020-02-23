using System.Threading.Tasks;

namespace PlexRequests.Api
{
    public interface IApiService
    {
        Task InvokeApiAsync(ApiRequest request);
        Task<T> InvokeApiAsync<T>(ApiRequest request);
    }
}
