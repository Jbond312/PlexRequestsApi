using MediatR;

namespace PlexRequests.ApiRequests.Plex.Commands
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
