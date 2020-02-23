using System.Threading.Tasks;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
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
        
        public async Task<PlexMediaItemRow> Get(int theMovieDbId)
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

        private async Task<PlexMediaItemRow> GetPlexMediaItem(AgentTypes agentType, string externalId)
        {
            return await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Movie, agentType, externalId);
        }
    }
}