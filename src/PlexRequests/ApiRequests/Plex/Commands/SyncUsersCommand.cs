using MediatR;

namespace PlexRequests.ApiRequests.Plex.Commands
{
    public class SyncUsersCommand : IRequest<ValidationContext>
    {
    }
}
