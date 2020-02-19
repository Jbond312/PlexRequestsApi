using MediatR;

namespace PlexRequests.ApiRequests.Health.Queries
{
    public class GetHealthQuery : IRequest<GetHealthQueryResult>
    {
    }
}
