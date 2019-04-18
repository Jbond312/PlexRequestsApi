using System;
using MongoDB.Bson.Serialization.Attributes;

namespace PlexRequests.Repository.Models
{
    public class PlexServerLibrary
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsArchived { get; set; }
    }
}
