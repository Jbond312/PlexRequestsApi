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
    }
}
