using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Plex.MediaItemRetriever
{
    public class TvMediaItemRetriever : IMediaItemRetriever
    {
        private readonly IPlexService _plexService;
        private readonly ITheMovieDbService _theMovieDbService;

        public TvMediaItemRetriever(
            IPlexService plexService,
            ITheMovieDbService theMovieDbService
        )
        {
            _plexService = plexService;
            _theMovieDbService = theMovieDbService;
        }

        public PlexMediaTypes MediaType => PlexMediaTypes.Show;

        public async Task<PlexMediaItem> Get(int theMovieDbId)
        {
            var plexMediaItem = await GetPlexMediaItem(AgentTypes.TheMovieDb, theMovieDbId.ToString());

            if (plexMediaItem == null)
            {
                var externalIds = await _theMovieDbService.GetTvExternalIds(theMovieDbId);

                if (!string.IsNullOrEmpty(externalIds.TvDb_Id))
                {
                    plexMediaItem = await GetPlexMediaItem(AgentTypes.TheTvDb, externalIds.TvDb_Id);
                }

                if (plexMediaItem == null && !string.IsNullOrEmpty(externalIds.Imdb_Id))
                {
                    plexMediaItem = await GetPlexMediaItem(AgentTypes.Imdb, externalIds.Imdb_Id);
                }
            }

            return plexMediaItem;
        }

        private async Task<PlexMediaItem> GetPlexMediaItem(AgentTypes agentType, string externalId)
        {
            return await _plexService.GetExistingMediaItemByAgent(PlexMediaTypes.Show, agentType, externalId);
        }
    }
}