using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PlexRequests.Core;
using PlexRequests.Core.Exceptions;

namespace PlexRequests.Api
{
    public class ApiService : IApiService
    {
        private readonly IPlexRequestsHttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            IPlexRequestsHttpClient httpClient,
            ILogger<ApiService> logger
            )
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task InvokeApiAsync(ApiRequest request)
        {
            using (var httpRequestMessage = CreateHttpRequestMessage(request))
            {
                var httpResponse =await _httpClient.SendAsync(httpRequestMessage);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    await LogApiUnsuccessful(request, httpResponse);
                }
            }
        }

        public async Task<T> InvokeApiAsync<T>(ApiRequest request)
        {
            using (var httpRequestMessage = CreateHttpRequestMessage(request))
            {
                var httpResponse = await _httpClient.SendAsync(httpRequestMessage);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    await LogApiUnsuccessful(request, httpResponse);
                }

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
                response = (T)serializer.Deserialize(reader);
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

        private async Task LogApiUnsuccessful(ApiRequest request, HttpResponseMessage httpResponse)
        {
            _logger.LogInformation($"StatusCode: {httpResponse.StatusCode}. Request Uri: {request.FullUri}");

            var rawResponse = await httpResponse.Content.ReadAsStringAsync();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(rawResponse);
            }

            throw new PlexRequestException("Unsuccessful response from 3rd Party API", httpResponse.StatusCode.ToString(), HttpStatusCode.FailedDependency);
        }
    }
}
