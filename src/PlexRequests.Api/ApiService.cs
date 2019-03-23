using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

                var response = DeserialiseResponse<T>(httpResponse, contentResponse);

                return response;
            }
        }

        private static T DeserialiseResponse<T>(HttpResponseMessage httpResponse, string contentResponse)
        {
            T response;
            if (httpResponse.Content?.Headers?.ContentType?.MediaType == "application/xml")
            {
                var serializer = new XmlSerializer(typeof(T));
                var reader = new StringReader(contentResponse);
                response = (T) serializer.Deserialize(reader);
            }
            else
            {
                response = JsonConvert.DeserializeObject<T>(contentResponse);
            }

            return response;
        }

        private static void AddRequestHeaders(HttpRequestMessage httpRequestMessage, Dictionary<string, string> headers)
        {
            foreach (var (key, value) in headers)
            {
                httpRequestMessage.Headers.Add(key, value);
            }
        }

        private static HttpRequestMessage CreateHttpRequestMessage(ApiRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri);
            AddRequestHeaders(httpRequestMessage, request.RequestHeaders);

            if (request.Body != null)
            {
                SetJsonBody(httpRequestMessage, request.Body);
            }

            return httpRequestMessage;
        }

        private static void SetJsonBody(HttpRequestMessage httpRequestMessage, object body)
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });

            httpRequestMessage.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            httpRequestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }
    }
}
