using System.Threading.Tasks;
using PlexRequests.Repository.Models;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Plex.MediaItemRetriever
{
    public interface IMediaItemRetriever
    {
        Task<PlexMediaItem> Get(int theMovieDbId);
        PlexMediaTypes MediaType { get; }
    }
}