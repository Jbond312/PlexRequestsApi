using System;
using System.Collections.Generic;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.ApiRequests.Issues.Models.Detail
{
    public class IssueDetailModel
    {
        public int Id { get; set; }
        public string MediaItemName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public IssueStatuses Status { get; set; }
        public string Resolution { get; set; }
        public DateTime Created { get; set; }
        public List<IssueCommentDetailModel> Comments { get; set; }
    }
}