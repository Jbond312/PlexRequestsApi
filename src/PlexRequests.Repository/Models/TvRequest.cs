using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Repository.Models
{
    public class TvRequest
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public MediaAgent PrimaryAgent { get; set; }
        public string PlexMediaUri { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; }
        public List<RequestSeason> Seasons { get; set; }
        public RequestStatuses Status { get; set; }
        public string ImagePath { get; set; }
        public DateTime AirDate { get; set; }
        public DateTime Created { get; set; }
        public List<MediaAgent> AdditionalAgents { get; set; }
        public string Comment { get; set; }
        public bool Track { get; set; }
    }
}