using System;

namespace PlexRequests.ApiRequests.Issues.DTOs.Detail
{
    public class IssueCommentDetailModel
    {
        public string UserName { get; set; }
        public DateTime Created { get; set; }
        public string Comment { get; set; }
    }
}