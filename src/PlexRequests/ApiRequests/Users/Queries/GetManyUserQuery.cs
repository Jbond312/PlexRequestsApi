using MediatR;

namespace PlexRequests.ApiRequests.Users.Queries
{
    public class GetManyUserQuery : IRequest<GetManyUserQueryResult>
    {
        public bool IncludeDisabled { get; set; }
    }
}
