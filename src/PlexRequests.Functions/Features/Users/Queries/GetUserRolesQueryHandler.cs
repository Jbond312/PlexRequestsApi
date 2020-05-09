using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Auth;

namespace PlexRequests.Functions.Features.Users.Queries
{
    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, GetUserRolesQueryResult>
    {
        public Task<GetUserRolesQueryResult> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            //TODO These should come from the database
            var roles = new List<string>
            {
                PlexRequestRoles.Admin,
                PlexRequestRoles.User,
                PlexRequestRoles.Commenter
            };

            var result = new GetUserRolesQueryResult
            {
                Roles = roles
            };

            return Task.FromResult(result);
        }
    }
}
