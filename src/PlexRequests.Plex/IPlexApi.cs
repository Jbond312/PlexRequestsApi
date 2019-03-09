using System.Threading.Tasks;
using PlexRequests.Plex.Models.OAuth;

namespace PlexRequests.Plex
{
    public interface IPlexApi
    {
        Task<OAuthPin> CreatePin();
    }
}
