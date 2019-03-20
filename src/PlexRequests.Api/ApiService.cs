using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PlexRequests.Api
{
    public class ApiService : IApiService
    {
        private readonly IPlexRequestsHttpClient _httpClient;

        public ApiService(IPlexRequestsHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task InvokeApiAsync(ApiRequest request)
        {
            using (var httpRequestMessage = CreateHttpRequestMessage(request))
            {
                await _httpClient.SendAsync(httpRequestMessage);
            }
        }

        public async Task<T> InvokeApiAsync<T>(ApiRequest request)
        {
            using (var httpRequestMessage = CreateHttpRequestMessage(request))
            {
                var httpResponse = await _httpClient.SendAsync(httpRequestMessage);

                var contentResponse = await httpResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(contentResponse);
            }
        }

        private static void AddRequestHeaders(HttpRequestMessage httpRequestMessage, Dictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        private static HttpRequestMessage CreateHttpRequestMessage(ApiRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri);
            AddRequestHeaders(httpRequestMessage, request.RequestHeaders);
            return httpRequestMessage;
        }
    }
}
