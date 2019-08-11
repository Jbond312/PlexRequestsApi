using System;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetOneIssueQuery : IRequest<GetOneIssueQueryResult>
    {
        public Guid Id { get; set; }

        public GetOneIssueQuery(Guid id)
        {
            Id = id;
        }
    }
}