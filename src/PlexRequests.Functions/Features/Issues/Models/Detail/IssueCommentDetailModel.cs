using System;

namespace PlexRequests.Functions.Features.Issues.Models.Detail
{
    public class IssueCommentDetailModel
    {
        public string UserName { get; set; }
        public DateTime Created { get; set; }
        public string Comment { get; set; }
        public int LikeCount { get; set; }
    }
}