using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Repository.Models
{
    public class Issue
    {
        [BsonId]
        public Guid Id { get; set; }
        public string MediaItemName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public MediaAgent MediaAgent { get; set; }
        public IssueStatuses Status { get; set; }
        public string Resolution { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; }
        public string ImagePath { get; set; }
        public DateTime AirDate { get; set; }
        public DateTime Created { get; set; }
        public List<IssueComment> Comments { get; set; }
    }
}