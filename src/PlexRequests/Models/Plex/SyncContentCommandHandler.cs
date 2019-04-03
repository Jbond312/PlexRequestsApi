using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Sync;

namespace PlexRequests.Models.Plex
{
    public class SyncContentCommandHandler : AsyncRequestHandler<SyncContentCommand>
    {
        private readonly IPlexSync _plexSync;

        public SyncContentCommandHandler(
            IPlexSync plexSync
            )
        {
            _plexSync = plexSync;
        }

        protected override async Task Handle(SyncContentCommand request, CancellationToken cancellationToken)
        {
            await _plexSync.Synchronise(request.FullRefresh);
        }
    }
}
