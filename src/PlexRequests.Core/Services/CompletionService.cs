using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class CompletionService : ICompletionService
    {
        private readonly IRequestService _requestService;

        public CompletionService(
            IRequestService requestService
            )
        {
            _requestService = requestService;
        }
        
        public async Task AutoCompleteRequests(Dictionary<RequestAgent, PlexMediaItem> agentsByPlexId, PlexMediaTypes mediaType)
        {
            var incompleteRequests = await _requestService.GetIncompleteRequests(mediaType);

            foreach (var incompleteRequest in incompleteRequests)
            {
                var allAgents =
                    new List<RequestAgent> {incompleteRequest.PrimaryAgent}.Concat(incompleteRequest.AdditionalAgents);
                
                foreach (var requestAgent in allAgents)
                {
                    if (!agentsByPlexId.TryGetValue(requestAgent, out var plexMediaItem))
                    {
                        continue;
                    }

                    incompleteRequest.PlexMediaUri = plexMediaItem.PlexMediaUri;
                    incompleteRequest.Status = RequestStatuses.Completed;

                    if (mediaType == PlexMediaTypes.Show)
                    {
                        AutoCompleteTvSeasonEpisodes(incompleteRequest, plexMediaItem);
                    }
                    
                    await _requestService.Update(incompleteRequest);
                    
                    break;
                }
            }
        }
        
        private static void AutoCompleteTvSeasonEpisodes(Request incompleteRequest, PlexMediaItem plexMediaItem)
        {
            var totalEpisodes = 0;
            var totalCompletedEpisodes = 0;
            var hasMissingSeasons = false;
            foreach (var season in incompleteRequest.Seasons)
            {
                var matchingSeason = plexMediaItem.Seasons.FirstOrDefault(x => x.Season == season.Index);

                if (matchingSeason == null)
                {
                    hasMissingSeasons = true;
                    continue;
                }

                season.PlexMediaUri = matchingSeason.PlexMediaUri;

                foreach (var episode in season.Episodes)
                {
                    totalEpisodes++;

                    if (episode.Status == RequestStatuses.Completed)
                    {
                        totalCompletedEpisodes++;
                        continue;
                    }
                    
                    var matchingEpisode = matchingSeason.Episodes.FirstOrDefault(x => x.Episode == episode.Index);

                    if (matchingEpisode == null)
                    {
                        continue;
                    }

                    episode.PlexMediaUri = matchingEpisode.PlexMediaUri;

                    totalCompletedEpisodes++;
                    episode.Status = RequestStatuses.Completed;
                }
            }

            if (totalCompletedEpisodes > 0)
            {
                if (hasMissingSeasons || totalCompletedEpisodes != totalEpisodes)
                {
                    incompleteRequest.Status = RequestStatuses.PartialCompletion;
                } else if (totalCompletedEpisodes == totalEpisodes)
                {
                    incompleteRequest.Status = RequestStatuses.Completed;
                }
            }
        }
    }
}