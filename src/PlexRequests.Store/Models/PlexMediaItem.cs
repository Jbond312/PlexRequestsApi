using System;
using MongoDB.Bson.Serialization.Attributes;
using PlexRequests.Store.Enums;

namespace PlexRequests.Store.Models
{
    public class PlexMediaItem
    {
        [BsonId]
        public Guid Id { get; set; }

        public int Key { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Resolution { get; set; }
        public bool IsArchived { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
    }
}
