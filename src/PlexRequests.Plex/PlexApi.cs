using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PlexRequests.Api;
using PlexRequests.Plex.Models;
using PlexRequests.Plex.Models.OAuth;
using PlexRequests.Settings;

namespace PlexRequests.Plex
{
    //https://github.com/Arcanemagus/plex-api/wiki/Plex.tv
    public class PlexApi : IPlexApi
    {
        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;
        private string _baseUri = "https://plex.tv/api/v2/";

        public PlexApi(
            IApiService apiService, 
            ISettingsService settingsService)
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

        public async Task<User> GetAccount(string authToken)
        {
            var apiRequest = new ApiRequestBuilder("https://plex.tv/users/account.json", "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .Build();

            var account = await _apiService.InvokeApiAsync<SignInAccount>(apiRequest);

            return account?.User;
        }

        public async Task<List<Server>> GetServers(string authToken)
        {
            var apiRequest = new ApiRequestBuilder("https://plex.tv/pms/servers.xml", "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .Build();

            var serverContainer = await _apiService.InvokeApiAsync<ServerContainer>(apiRequest);

            return serverContainer?.Servers;
        }

        public async Task<List<Friend>> GetFriends(string authToken)
        {
            var apiRequest = new ApiRequestBuilder("https://plex.tv/pms/friends/all", "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .Build();

            var friendContainer = await _apiService.InvokeApiAsync<FriendContainer>(apiRequest);

            return friendContainer?.Friends.ToList();
        }

        public async Task<PlexMediaContainer> GetLibraries(string authToken, string plexServerHost)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, "library/sections", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetLibrary(string authToken, string plexServerHost, string key)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, $"library/sections/{key}/all", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetMetadata(string authToken, string plexServerHost, int metadataId)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, $"library/metadata/{metadataId}", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetChildrenMetadata(string authToken, string plexServerHost, int metadataId)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, $"library/metadata/{metadataId}/children", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetPlexInfo(string authToken, string plexServerHost)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(await GetPlexHeaders())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
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
