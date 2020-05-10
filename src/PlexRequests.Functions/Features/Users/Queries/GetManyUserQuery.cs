using MediatR;

namespace PlexRequests.Functions.Features.Users.Queries
{
    public class GetManyUserQuery : IRequest<ValidationContext<GetManyUserQueryResult>>
    {
        public bool IncludeDisabled { get; set; }
    }
}
