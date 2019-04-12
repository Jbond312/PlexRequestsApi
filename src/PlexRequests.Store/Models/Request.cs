using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using PlexRequests.Store.Enums;

namespace PlexRequests.Store.Models
{
    public class Request
    {
        [BsonId] 
        public Guid Id { get; set; }
        public string Title { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
        public int? PlexRatingKey { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; }
        public List<RequestSeason> Seasons { get; set; }
        public bool IsApproved { get; set; }
        public string ImagePath { get; set; }
        public DateTime AirDate { get; set; }
    }
}