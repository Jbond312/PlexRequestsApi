using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.Functions.Features.Issues.Queries
{
    public class GetPagedIssueQuery : IRequest<ValidationContext<GetPagedIssueQueryResult>>
    {
        [Range(1, int.MaxValue)]
        public int? Page { get; set; }
        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }
        public bool IncludeResolved { get; set; }
    }
}