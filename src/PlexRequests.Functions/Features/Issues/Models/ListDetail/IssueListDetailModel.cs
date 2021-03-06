using System;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Issues.Models.ListDetail
{
    public class IssueListDetailModel
    {
        public int Id { get; set; }
        public string MediaItemName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public IssueStatuses Status { get; set; }
        public string Resolution { get; set; }
        public DateTime Created { get; set; }
    }
}