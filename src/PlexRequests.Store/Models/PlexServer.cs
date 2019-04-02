using System;
using System.Collections.Generic;
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
        public string LocalIp { get; set; }
        public int LocalPort { get; set; }
        public string ExternalIp { get; set;}
        public int ExternalPort { get; set; }
        public List<PlexServerLibrary> Libraries { get; set; }

        public string GetPlexUri(bool useLocal)
        {
            var uri = useLocal ? $"{Scheme}://{LocalIp}:{LocalPort}" : $"{Scheme}://{ExternalIp}:{ExternalPort}";

            return uri;
        }
    }
}
