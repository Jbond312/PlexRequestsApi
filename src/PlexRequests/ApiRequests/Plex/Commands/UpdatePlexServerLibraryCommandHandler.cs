using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.DataAccess;
using PlexRequests.Plex;

namespace PlexRequests.ApiRequests.Plex.Commands
{
    public class UpdatePlexServerLibraryCommandHandler : AsyncRequestHandler<UpdatePlexServerLibraryCommand>
    {
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePlexServerLibraryCommandHandler(
            IPlexService plexService,
            IUnitOfWork unitOfWork
            )
        {
            _plexService = plexService;
            _unitOfWork = unitOfWork;
        }
        
        protected override async Task Handle(UpdatePlexServerLibraryCommand request, CancellationToken cancellationToken)
        {
            var server = await _plexService.GetServer();

            var libraryToUpdate = server.PlexLibraries.FirstOrDefault(x => x.LibraryKey == request.Key && !x.IsArchived);

            if (libraryToUpdate == null)
            {
                throw new PlexRequestException("Invalid library key", "No library was found for the given key", HttpStatusCode.NotFound);
            }

            libraryToUpdate.IsEnabled = request.IsEnabled;

            await _unitOfWork.CommitAsync();
        }
    }
}