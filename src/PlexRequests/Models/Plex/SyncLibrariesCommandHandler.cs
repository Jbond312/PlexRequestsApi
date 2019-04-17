using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using PlexRequests.Models.SubModels.Detail;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Settings;
using PlexRequests.Store.Models;

namespace PlexRequests.Models.Plex
{
    public class SyncLibrariesCommandHandler : IRequestHandler<SyncLibrariesCommand, SyncLibrariesCommandResult>
    {
        private readonly IMapper _mapper;
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly PlexSettings _plexSettings;

        public SyncLibrariesCommandHandler(
            IMapper mapper,
            IPlexApi plexApi,
            IPlexService plexService,
            IOptions<PlexSettings> plexSettings)
        {
            _mapper = mapper;
            _plexApi = plexApi;
            _plexService = plexService;
            _plexSettings = plexSettings.Value;
        }

        public async Task<SyncLibrariesCommandResult> Handle(SyncLibrariesCommand request, CancellationToken cancellationToken)
        {
            var server = await _plexService.GetServer();

            var libraryContainer = await _plexApi.GetLibraries(server.AccessToken, server.GetPlexUri(_plexSettings.ConnectLocally));

            CheckForDeletedLibraries(server, libraryContainer);

            SetNewLibraries(libraryContainer, server);

            await _plexService.Update(server);

            var libraries = _mapper.Map<List<PlexServerLibraryDetailModel>>(server.Libraries);

            return new SyncLibrariesCommandResult
            {
                Libraries = libraries
            };
        }

        private static void SetNewLibraries(PlexMediaContainer libraryContainer, PlexServer server)
        {
            var newLibraries =
                libraryContainer.MediaContainer.Directory.Where(rl =>
                    !server.Libraries.Select(x => x.Key).Contains(rl.Key)).Select(nl => new PlexServerLibrary
                {
                    Key = nl.Key,
                    Title = nl.Title,
                    Type = nl.Type,
                    IsEnabled = false
                });

            server.Libraries.AddRange(newLibraries);
        }

        private static void CheckForDeletedLibraries(PlexServer server, PlexMediaContainer libraryContainer)
        {
            foreach (var existingLibrary in server.Libraries.Where(x => !x.IsArchived))
            {
                var libraryExists = libraryContainer.MediaContainer.Directory.Any(x => x.Key == existingLibrary.Key);

                if (libraryExists)
                {
                    continue;
                }

                existingLibrary.IsArchived = true;
                existingLibrary.IsEnabled = false;
            }
        }
    }
}
