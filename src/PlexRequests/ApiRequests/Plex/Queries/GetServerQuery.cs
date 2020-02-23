using MediatR;

namespace PlexRequests.ApiRequests.Plex.Queries
{
    public class GetServerQuery : IRequest<GetPlexServerQueryResult>
    {
    }
}
