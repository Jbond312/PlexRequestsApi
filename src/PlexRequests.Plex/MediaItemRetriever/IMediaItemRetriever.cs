using System.Threading.Tasks;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Plex.MediaItemRetriever
{
    public interface IMediaItemRetriever
    {
        Task<PlexMediaItemRow> Get(int theMovieDbId);
        PlexMediaTypes MediaType { get; }
    }
}