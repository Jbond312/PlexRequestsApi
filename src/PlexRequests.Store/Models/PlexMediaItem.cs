using System;
using MongoDB.Bson.Serialization.Attributes;

namespace PlexRequests.Store.Models
{
    public class PlexMediaItem
    {
        [BsonId]
        public Guid Id { get; set; }

        public string Key { get; set; }
        public string Title { get; set; }
        public string Guid { get; set; }
    }
}
