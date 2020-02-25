using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.DataAccess;
using PlexRequests.Plex;

namespace PlexRequests.ApiRequests.Plex.Commands
{
    public class UpdatePlexServerLibraryCommandHandler : IRequestHandler<UpdatePlexServerLibraryCommand, ValidationContext>
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
        
        public async Task<ValidationContext> Handle(UpdatePlexServerLibraryCommand request, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext();

            var server = await _plexService.GetServer();

            if (server == null)
            {
                resultContext.AddError("No admin server found", "Cannot update plex library as no admin server has been found");
                return resultContext;
            }

            var libraryToUpdate = server.PlexLibraries.FirstOrDefault(x => x.LibraryKey == request.Key && !x.IsArchived);

            resultContext.AddErrorIf(() => libraryToUpdate == null, "Invalid library key", "No library was found for the given key");

            if(!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            // ReSharper disable once PossibleNullReferenceException
            libraryToUpdate.IsEnabled = request.IsEnabled;

            await _unitOfWork.CommitAsync();

            return resultContext;
        }
    }
}