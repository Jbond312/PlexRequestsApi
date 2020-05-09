using MediatR;

namespace PlexRequests.Functions.Features.Plex.Commands
{
    public class SyncContentCommand : IRequest
    {
        public bool FullRefresh { get; }

        public SyncContentCommand(bool fullRefresh)
        {
            FullRefresh = fullRefresh;
        }
    }
}
