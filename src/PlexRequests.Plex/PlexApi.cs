using System;
using System.Net.Http;
using System.Threading.Tasks;
using PlexRequests.Api;
using PlexRequests.Plex.Models;
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

        public async Task<OAuthPin> GetPin(int pinId)
        {
            var apiRequest =
                new ApiRequestBuilder(_baseUri, $"pins/{pinId}", HttpMethod.Get)
                    .AcceptJson()
                    .AddHeader("X-Plex-Client-Identifier", Guid.NewGuid().ToString("N"))
                    .Build();

            var oauthPin = await _apiService.InvokeApiAsync<OAuthPin>(apiRequest);

            return oauthPin;
        }

        public async Task<User> SignIn(string username, string password)
        {
            var signInRequest = new SignInRequest
            {
                User = new UserRequest
                {
                    Login = username,
                    Password = password
                }
            };

            var apiRequest =
                new ApiRequestBuilder("https://plex.tv/users/sign_in.json", "", HttpMethod.Post)
                    .AddHeader("X-Plex-Client-Identifier", Guid.NewGuid().ToString("N"))
                    .AcceptJson()
                    .AddJsonBody(signInRequest)
                    .Build();

            var account = await _apiService.InvokeApiAsync<SignInAccount>(apiRequest);

            return account?.User;
        }
    }
}
