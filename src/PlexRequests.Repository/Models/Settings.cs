﻿using System;
using MongoDB.Bson.Serialization.Attributes;

namespace PlexRequests.Repository.Models
{
    public class Settings
    {
        [BsonId]
        public Guid Id { get; set; }
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public Guid PlexClientId { get; set; }
        public bool OverwriteSettings { get; set; }
    }
}
