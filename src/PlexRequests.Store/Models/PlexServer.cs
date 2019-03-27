using System;
using MongoDB.Bson.Serialization.Attributes;

namespace PlexRequests.Store.Models
{
    public class PlexServer
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public string MachineIdentifier { get; set; }
        public string Scheme { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
    }
}
