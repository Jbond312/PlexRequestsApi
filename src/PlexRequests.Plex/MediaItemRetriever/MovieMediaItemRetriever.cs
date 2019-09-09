using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Plex.MediaItemRetriever
{
    public class MovieMediaItemRetriever : IMediaItemRetriever
    {
        private readonly IPlexService _plexService;
        private readonly ITheMovieDbService _theMovieDbService;

        public MovieMediaItemRetriever(
            IPlexService plexService,
            ITheMovieDbService theMovieDbService
        )
        {
            _plexService = plexService;
            _theMovieDbService = theMovieDbService;
        }

        public PlexMediaTypes MediaType => PlexMediaTypes.Movie;

        public async Task<PlexMediaItem> Get(int theMovieDbId)
        {
            var plexMediaItem = await GetPlexMediaItem(AgentTypes.TheMovieDb, theMovieDbId.ToString());

            if (plexMediaItem == null)
            {
                var externalIds = await _theMovieDbService.GetMovieExternalIds(theMovieDbId);

                if (!string.IsNullOrEmpty(externalIds.Imdb_Id))
                {
                    plexMediaItem = await GetPlexMediaItem(AgentTypes.Imdb, externalIds.Imdb_Id);
                }
            }

            return plexMediaItem;
        }

        private async Task<PlexMediaItem> GetPlexMediaItem(AgentTypes agentType, string externalId)
        {
            return await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Movie, agentType, externalId);
        }
    }
}