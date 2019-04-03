using MediatR;

namespace PlexRequests.Models.Plex
{
    public class SyncLibrariesCommand : IRequest<SyncLibrariesCommandResult>
    {
    }
}
