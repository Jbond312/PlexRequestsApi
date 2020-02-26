using MediatR;

namespace PlexRequests.ApiRequests.Plex.Commands
{
    public class SyncLibrariesCommand : IRequest<ValidationContext>
    {
    }
}
