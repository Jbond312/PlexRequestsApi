using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using PlexRequests.Core.ExtensionMethods;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;

namespace PlexRequests.ApiRequests.Plex.Commands
{
    public class SyncLibrariesCommandHandler : AsyncRequestHandler<SyncLibrariesCommand>
    {
        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PlexSettings _plexSettings;

        public SyncLibrariesCommandHandler(
            IPlexApi plexApi,
            IPlexService plexService,
            IUnitOfWork unitOfWork,
            IOptionsSnapshot<PlexSettings> plexSettings)
        {
            _plexApi = plexApi;
            _plexService = plexService;
            _unitOfWork = unitOfWork;
            _plexSettings = plexSettings.Value;
        }

        protected override async Task Handle(SyncLibrariesCommand request, CancellationToken cancellationToken)
        {
            var server = await _plexService.GetServer();

            var libraryContainer = await _plexApi.GetLibraries(server.AccessToken, server.GetPlexUri(_plexSettings.ConnectLocally));

            CheckForDeletedLibraries(server, libraryContainer);

            SetNewLibraries(libraryContainer, server);
            
            await _unitOfWork.CommitAsync();
        }

        private static void SetNewLibraries(PlexMediaContainer libraryContainer, PlexServerRow server)
        {
            var newLibraries =
                libraryContainer.MediaContainer.Directory.Where(rl =>
                    !server.PlexLibraries.Select(x => x.LibraryKey).Contains(rl.Key)).Select(nl => new PlexLibraryRow
                {
                    LibraryKey = nl.Key,
                    Title = nl.Title,
                    Type = nl.Type,
                    IsEnabled = false
                });


            foreach (var newLibrary in newLibraries)
            {
                server.PlexLibraries.Add(newLibrary);
            }
        }

        private static void CheckForDeletedLibraries(PlexServerRow server, PlexMediaContainer libraryContainer)
        {
            foreach (var existingLibrary in server.PlexLibraries.Where(x => !x.IsArchived))
            {
                var libraryExists = libraryContainer.MediaContainer.Directory.Any(x => x.Key == existingLibrary.LibraryKey);

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
