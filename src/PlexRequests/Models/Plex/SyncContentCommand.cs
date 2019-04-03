using MediatR;

namespace PlexRequests.Models.Plex
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
