using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Issues.Queries
{
    public class GetOneIssueQuery : IRequest<ValidationContext<GetOneIssueQueryResult>>
    {
        [Required]
        public int Id { get; set; }

        public GetOneIssueQuery(int id)
        {
            Id = id;
        }
    }
}