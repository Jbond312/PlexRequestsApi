using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using PlexRequests.Functions.Features.Health.Queries;
using RestSharp;

namespace PlexRequests.IntegrationTests
{
    public class PlexRequestsIntegrationTests
    {
        private readonly RestClient _restClient;

        public PlexRequestsIntegrationTests()
        {
            _restClient = new RestClient(TestHostFixture.BaseUrl);

        }
        
        public Task<IRestResponse<GetHealthQueryResult>> GetHealth()
        {
            var request = new RestRequest("/api/health", Method.GET);

            return _restClient.ExecuteGetAsync<GetHealthQueryResult>(request);
        }

        public async Task<IRestResponse> CallTimerTriggerFunction(string functionName)
        {
            var route = $"/admin/functions/{functionName}";

            dynamic body = new
            {
                input = ""
            };

            var request = new RestRequest(route, Method.POST)
                .AddJsonBody(body);

            var response = await _restClient.ExecuteAsync(request);

            return response;
        }

        public void VerifyResponseCode(IRestResponse restResponse, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            restResponse.StatusCode.Should().Be(expectedStatusCode);
        }

        public void VerifyMessageInResponseContent(IRestResponse restResponse, string expectedMessage)
        {
            var message = JsonConvert.DeserializeObject(restResponse.Content);
            message.Should().Be(expectedMessage);
        }
    }
}
