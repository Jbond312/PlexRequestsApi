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
        Task<PlexMediaContainer> GetLibrarySections(string authToken, string plexServerHost);
    }
}
