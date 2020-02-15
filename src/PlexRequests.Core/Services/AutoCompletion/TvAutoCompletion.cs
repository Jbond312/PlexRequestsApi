using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Services.AutoCompletion
{
    public class TvAutoCompletion : IAutoComplete
    {
        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        public PlexMediaTypes MediaType => PlexMediaTypes.Show;

        public TvAutoCompletion(
            ITvRequestService requestService,
            IUnitOfWork unitOfWork
            )
        {
            _requestService = requestService;
            _unitOfWork = unitOfWork;
        }

        public async Task AutoComplete(Dictionary<MediaAgent, PlexMediaItemRow> agentsByPlexId)
        {
            var incompleteRequests = await _requestService.GetIncompleteRequests();

            foreach (var incompleteRequest in incompleteRequests)
            {
                var allAgents = incompleteRequest.TvRequestAgents.Select(x => new MediaAgent(x.AgentType, x.AgentSourceId));

                foreach (var requestAgent in allAgents)
                {
                    if (!agentsByPlexId.TryGetValue(requestAgent, out var plexMediaItem))
                    {
                        continue;
                    }

                    incompleteRequest.PlexMediaItem = plexMediaItem;
                    incompleteRequest.RequestStatus = RequestStatuses.Completed;

                    if (incompleteRequest.Track)
                    {
                        continue;
                    }

                    AutoCompleteTvSeasonEpisodes(incompleteRequest, plexMediaItem);
                    
                    break;
                }
            }

            await _unitOfWork.CommitAsync();
        }

        private void AutoCompleteTvSeasonEpisodes(TvRequestRow incompleteRequest, PlexMediaItemRow plexMediaItem)
        {
            foreach (var season in incompleteRequest.TvRequestSeasons)
            {
                var matchingSeason = plexMediaItem.PlexSeasons.FirstOrDefault(x => x.Season == season.SeasonIndex);

                if (matchingSeason == null)
                {
                    continue;
                }

                season.PlexSeason = matchingSeason;

                foreach (var episode in season.TvRequestEpisodes)
                {
                    if (episode.RequestStatus == RequestStatuses.Completed)
                    {
                        continue;
                    }

                    var matchingEpisode = matchingSeason.PlexEpisodes.FirstOrDefault(x => x.Episode == episode.EpisodeIndex);

                    if (matchingEpisode == null)
                    {
                        continue;
                    }

                    episode.PlexEpisode = matchingEpisode;
                    episode.RequestStatus = RequestStatuses.Completed;
                }
            }

            _requestService.SetAggregatedStatus(incompleteRequest);
        }
    }
}
