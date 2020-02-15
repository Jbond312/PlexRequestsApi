using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PlexRequests.Api;
using PlexRequests.Core.Settings;
using PlexRequests.Plex.Models;
using PlexRequests.Plex.Models.OAuth;

namespace PlexRequests.Plex
{
    //https://github.com/Arcanemagus/plex-api/wiki/Plex.tv
    public class PlexApi : IPlexApi
    {
        private readonly IApiService _apiService;
        private readonly PlexRequestsSettings _settings;
        private readonly string _baseUri = "https://plex.tv/api/v2/";

        public PlexApi(
            IApiService apiService,
            IOptions<PlexRequestsSettings> settings)
        {
            _apiService = apiService;
            _settings = settings.Value;
        }

        public async Task<OAuthPin> CreatePin()
        {
            var apiRequest =
                new ApiRequestBuilder(_baseUri, "pins", HttpMethod.Post)
                    .AcceptJson()
                    .AddQueryParam("strong", "true")
                    .AddRequestHeaders(GetClientIdentifierHeader())
                    .AddRequestHeaders(GetClientMetaHeaders())
                    .Build();

            var oauthPin = await _apiService.InvokeApiAsync<OAuthPin>(apiRequest);

            return oauthPin;
        }

        public async Task<OAuthPin> GetPin(int pinId)
        {
            var apiRequest =
                new ApiRequestBuilder(_baseUri, $"pins/{pinId}", HttpMethod.Get)
                    .AcceptJson()
                    .AddRequestHeaders(GetClientIdentifierHeader())
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
                    .AddRequestHeaders(GetClientIdentifierHeader())
                    .AddRequestHeaders(GetClientMetaHeaders())
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
                .AddRequestHeaders(GetClientIdentifierHeader())
                .Build();

            var account = await _apiService.InvokeApiAsync<SignInAccount>(apiRequest);

            return account?.User;
        }

        public async Task<List<Server>> GetServers(string authToken)
        {
            var apiRequest = new ApiRequestBuilder("https://plex.tv/pms/servers.xml", "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .Build();

            var serverContainer = await _apiService.InvokeApiAsync<ServerContainer>(apiRequest);

            return serverContainer?.Servers;
        }

        public async Task<List<Friend>> GetFriends(string authToken)
        {
            var apiRequest = new ApiRequestBuilder("https://plex.tv/pms/friends/all", "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .Build();

            var friendContainer = await _apiService.InvokeApiAsync<FriendContainer>(apiRequest);

            return friendContainer?.Friends.ToList();
        }

        public async Task<PlexMediaContainer> GetLibraries(string authToken, string plexServerHost)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, "library/sections", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetLibrary(string authToken, string plexServerHost, string key)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, $"library/sections/{key}/all", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetRecentlyAdded(string authToken, string plexServerHost, string key)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, $"library/sections/{key}/recentlyAdded", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetMetadata(string authToken, string plexServerHost, int metadataId)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, $"library/metadata/{metadataId}", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetChildrenMetadata(string authToken, string plexServerHost, int metadataId)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, $"library/metadata/{metadataId}/children", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        public async Task<PlexMediaContainer> GetPlexInfo(string authToken, string plexServerHost)
        {
            var apiRequest = new ApiRequestBuilder(plexServerHost, "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddRequestHeaders(GetClientIdentifierHeader())
                .AcceptJson()
                .Build();

            var plexMediaContainer = await _apiService.InvokeApiAsync<PlexMediaContainer>(apiRequest);

            return plexMediaContainer;
        }

        private Dictionary<string, string> GetClientIdentifierHeader()
        {
            var plexHeaders = new Dictionary<string, string>
            {
                ["X-Plex-Client-Identifier"] = _settings.PlexRequestsClientId
            };

            return plexHeaders;
        }

        private Dictionary<string, string> GetClientMetaHeaders()
        {
            var plexHeaders = new Dictionary<string, string>
            {
                ["X-Plex-Product"] = _settings.ApplicationName,
                ["X-Plex-Version"] = _settings.Version,
                ["X-Plex-Device"] = "Web",
                ["X-Plex-Platform"] = "Web"
            };

            return plexHeaders;
        }
    }
}
