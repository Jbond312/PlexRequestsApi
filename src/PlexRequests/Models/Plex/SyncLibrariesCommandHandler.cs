﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using PlexRequests.Core.Settings;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Models;

namespace PlexRequests.Models.Plex
{
    public class SyncLibrariesCommandHandler : AsyncRequestHandler<SyncLibrariesCommand>
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly PlexSettings _plexSettings;

        public SyncLibrariesCommandHandler(
            IPlexApi plexApi,
            IPlexService plexService,
            IOptions<PlexSettings> plexSettings)
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _plexSettings = plexSettings.Value;
        }

        protected override async Task Handle(SyncLibrariesCommand request, CancellationToken cancellationToken)
        {
            var server = await _plexService.GetServer();

            var libraryContainer = await _plexApi.GetLibraries(server.AccessToken, server.GetPlexUri(_plexSettings.ConnectLocally));

            CheckForDeletedLibraries(server, libraryContainer);

            SetNewLibraries(libraryContainer, server);

            await _plexService.Update(server);
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
