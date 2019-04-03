using MediatR;

namespace PlexRequests.Models.Plex
{
    public class GetServerQuery : IRequest<GetPlexServerQueryResult>
    {
    }
}
