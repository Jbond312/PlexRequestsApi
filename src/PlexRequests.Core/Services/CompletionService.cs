using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Services.AutoCompletion;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class CompletionService : ICompletionService
    {
        private readonly ILogger<CompletionService> _logger;

        private readonly List<IAutoComplete> _autoCompleters;

        public CompletionService(
            IMovieRequestService movieRequestService,
            ITvRequestService tvRequestService,
            ILogger<CompletionService> logger
        )
        {
            _logger = logger;
            _autoCompleters = new List<IAutoComplete>
            {
                new MovieAutoCompletion(movieRequestService),
                new TvAutoCompletion(tvRequestService)
            };
        }

        public async Task AutoCompleteRequests(Dictionary<MediaAgent, PlexMediaItem> agentsByPlexId,
            PlexMediaTypes mediaType)
        {
            var completer = _autoCompleters.FirstOrDefault(x => x.MediaType == mediaType);

            if (completer == null)
            {
                _logger.LogError($"Attempt to execute auto completion on PlexMediaType '{mediaType}' that has no configured auto complete implementation.");
                return;
            }

            await completer.AutoComplete(agentsByPlexId);
        }
    }
}