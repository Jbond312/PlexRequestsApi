using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services.AutoCompletion
{
    public class MovieAutoCompletion : IAutoComplete
    {
        private readonly IMovieRequestService _requestService;

        public PlexMediaTypes MediaType => PlexMediaTypes.Movie;
        
        public MovieAutoCompletion(
            IMovieRequestService requestService
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

                    await _requestService.Update(incompleteRequest);

                    break;
                }
            }
        }
    }
}
