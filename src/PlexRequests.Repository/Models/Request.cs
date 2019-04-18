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
        public RequestAgent PrimaryAgent { get; set; }
        public string PlexMediaUri { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; }
        public List<RequestSeason> Seasons { get; set; }
        public RequestStatuses Status { get; set; }
        public string ImagePath { get; set; }
        public DateTime AirDate { get; set; }
        public DateTime Created { get; set; }
        public List<RequestAgent> AdditionalAgents { get; set; }
    }
}