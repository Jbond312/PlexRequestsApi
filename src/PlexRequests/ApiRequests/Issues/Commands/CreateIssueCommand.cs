using MediatR;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class CreateIssueCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}