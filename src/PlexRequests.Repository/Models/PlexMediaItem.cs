using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using PlexRequests.Store.Enums;

namespace PlexRequests.Store.Models
{
    public class PlexMediaItem : BasePlexMediaItem
    {
        public PlexMediaItem()
        {
            Seasons = new List<PlexSeason>();
        }

        [BsonId]
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string Resolution { get; set; }
        public PlexMediaTypes MediaType { get; set; }
        public List<PlexSeason> Seasons { get; set; }
    }
}
