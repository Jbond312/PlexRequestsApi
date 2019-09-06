using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services.AutoCompletion
{
    public class TvAutoCompletion : IAutoComplete
    {
        private readonly ITvRequestService _requestService;

        public PlexMediaTypes MediaType => PlexMediaTypes.Show;
        
        public TvAutoCompletion(
            ITvRequestService requestService
            )
        {
            _requestService = requestService;
        }
        
        public async Task AutoComplete(Dictionary<MediaAgent, PlexMediaItem> agentsByPlexId)
        {
            var incompleteRequests = await _requestService.GetIncompleteRequests();

            foreach (var incompleteRequest in incompleteRequests)
            {
                var allAgents =
                    new List<MediaAgent> { incompleteRequest.PrimaryAgent }.Concat(incompleteRequest.AdditionalAgents);

                foreach (var requestAgent in allAgents)
                {
                    if (!agentsByPlexId.TryGetValue(requestAgent, out var plexMediaItem))
                    {
                        continue;
                    }

                    incompleteRequest.PlexMediaUri = plexMediaItem.PlexMediaUri;
                    incompleteRequest.Status = RequestStatuses.Completed;

                    if (incompleteRequest.Track)
                    {
                        continue;
                    }

                    AutoCompleteTvSeasonEpisodes(incompleteRequest, plexMediaItem);


                    await _requestService.Update(incompleteRequest);

                    break;
                }
            }
        }

        private void AutoCompleteTvSeasonEpisodes(TvRequest incompleteRequest, PlexMediaItem plexMediaItem)
        {
            foreach (var season in incompleteRequest.Seasons)
            {
                var matchingSeason = plexMediaItem.Seasons.FirstOrDefault(x => x.Season == season.Index);

                if (matchingSeason == null)
                {
                    continue;
                }

                season.PlexMediaUri = matchingSeason.PlexMediaUri;

                foreach (var episode in season.Episodes)
                {
                    if (episode.Status == RequestStatuses.Completed)
                    {
                        continue;
                    }

                    var matchingEpisode = matchingSeason.Episodes.FirstOrDefault(x => x.Episode == episode.Index);

                    if (matchingEpisode == null)
                    {
                        continue;
                    }

                    episode.PlexMediaUri = matchingEpisode.PlexMediaUri;
                    episode.Status = RequestStatuses.Completed;
                }
            }

            incompleteRequest.Status = _requestService.CalculateAggregatedStatus(incompleteRequest);
        }
    }
}
