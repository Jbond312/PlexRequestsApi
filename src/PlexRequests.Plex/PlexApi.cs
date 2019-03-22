using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PlexRequests.Api;
using PlexRequests.Plex.Models;
using PlexRequests.Plex.Models.OAuth;
using PlexRequests.Settings;

namespace PlexRequests.Plex
{
    public class PlexApi : IPlexApi
    {
        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;
        private string _baseUri = "https://plex.tv/api/v2/";

        public PlexApi(IApiService apiService, ISettingsService settingsService)
        {
            _apiService = apiService;
            _settingsService = settingsService;
        }

        public async Task<OAuthPin> CreatePin()
        {
            var apiRequest =
                new ApiRequestBuilder(_baseUri, "pins", HttpMethod.Post)
                    .AcceptJson()
                    .AddQueryParam("strong", "true")
                    .AddRequestHeaders(await GetPlexHeaders())
                    .Build();

            var oauthPin = await _apiService.InvokeApiAsync<OAuthPin>(apiRequest);

            return oauthPin;
        }

        public async Task<OAuthPin> GetPin(int pinId)
        {
            var apiRequest =
                new ApiRequestBuilder(_baseUri, $"pins/{pinId}", HttpMethod.Get)
                    .AcceptJson()
                    .AddRequestHeaders(await GetPlexHeaders())
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
                    .AddRequestHeaders(await GetPlexHeaders())
                    .AcceptJson()
                    .AddJsonBody(signInRequest)
                    .Build();

            var account = await _apiService.InvokeApiAsync<SignInAccount>(apiRequest);

            return account?.User;
        }

        private async Task<Dictionary<string, string>> GetPlexHeaders()
        {
            var plexSettings = await _settingsService.Get();

            var plexHeaders = new Dictionary<string, string>
            {
                ["X-Plex-Client-Identifier"] = plexSettings.PlexClientId.ToString("N")
            };

            return plexHeaders;
        }
    }
}
