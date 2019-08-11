using System;

namespace PlexRequests.Repository.Models
{
    public class IssueComment
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }
        public string Comment { get; set; }
    }
}