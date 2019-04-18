using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Repository.Models
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
