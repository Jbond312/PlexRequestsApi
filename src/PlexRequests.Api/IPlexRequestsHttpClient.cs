using System.Net.Http;
using System.Threading.Tasks;

namespace PlexRequests.Api
{
    public interface IPlexRequestsHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
