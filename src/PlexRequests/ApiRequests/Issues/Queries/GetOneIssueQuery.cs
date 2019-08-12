using System;
using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Issues.Queries
{
    public class GetOneIssueQuery : IRequest<GetOneIssueQueryResult>
    {
        [Required]
        public Guid Id { get; set; }

        public GetOneIssueQuery(Guid id)
        {
            Id = id;
        }
    }
}