using System;
using System.Collections.Generic;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Issues.Models.Detail
{
    public class IssueDetailModel
    {
        public Guid Id { get; set; }
        public string MediaItemName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public IssueStatuses Status { get; set; }
        public string Resolution { get; set; }
        public string ImagePath { get; set; }
        public DateTime AirDate { get; set; }
        public DateTime Created { get; set; }
        public List<IssueCommentDetailModel> Comments { get; set; }
    }
}