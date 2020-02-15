using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Services.AutoCompletion
{
    public class MovieAutoCompletion : IAutoComplete
    {
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        public PlexMediaTypes MediaType => PlexMediaTypes.Movie;
        
        public MovieAutoCompletion(
            IMovieRequestService requestService,
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
                var allAgents = incompleteRequest.MovieRequestAgents.Select(x => new MediaAgent(x.AgentType, x.AgentSourceId));

                foreach (var requestAgent in allAgents)
                {
                    if (!agentsByPlexId.TryGetValue(requestAgent, out var plexMediaItem))
                    {
                        continue;
                    }

                    incompleteRequest.PlexMediaItem.MediaUri = plexMediaItem.MediaUri;
                    incompleteRequest.RequestStatus = RequestStatuses.Completed;

                    break;
                }
            }

            await _unitOfWork.CommitAsync();
        }
    }
}
