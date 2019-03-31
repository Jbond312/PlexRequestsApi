using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Plex.Models;
using PlexRequests.Plex.Models.OAuth;

namespace PlexRequests.Plex
{
    public interface IPlexApi
    {
        Task<OAuthPin> CreatePin();
        Task<OAuthPin> GetPin(int pinId);
        Task<User> SignIn(string username, string password);
        Task<User> GetAccount(string authToken);
        Task<List<Server>> GetServers(string authToken);
        Task<List<Friend>> GetFriends(string authToken);
        Task<PlexMediaContainer> GetLibraries(string authToken, string plexServerHost);
        Task<PlexMediaContainer> GetLibrary(string authToken, string plexServerHost, string key);
        Task<PlexMediaContainer> GetRecentlyAdded(string authToken, string plexServerHost, string key);
        Task<PlexMediaContainer> GetMetadata(string authToken, string plexServerHost, int metadataId);
        Task<PlexMediaContainer> GetChildrenMetadata(string authToken, string plexServerHost, int parentMetadataId);
        Task<PlexMediaContainer> GetPlexInfo(string authToken, string plexServerHost);
    }
}
