using System;
using System.Net.Http;
using System.Threading.Tasks;
using PlexRequests.Api;
using PlexRequests.Plex.Models.OAuth;

namespace PlexRequests.Plex
{
    public class PlexApi : IPlexApi
    {
        private readonly IApiService _apiService;
        private string _baseUri = "https://plex.tv/api/v2/";

        public PlexApi(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<OAuthPin> CreatePin()
        {
            var apiRequest =
                new ApiRequestBuilder(_baseUri, "pins?strong=true", HttpMethod.Post)
                    .AcceptJson()
                    .AddHeader("X-Plex-Client-Identifier", Guid.NewGuid().ToString("N"))
                    .Build();

            var oauthPin = await _apiService.InvokeApiAsync<OAuthPin>(apiRequest);

            return oauthPin;
        }
    }
}
