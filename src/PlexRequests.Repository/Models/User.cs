using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace PlexRequests.Repository.Models
{
    public class User
    {
        [BsonId]
        public Guid Id { get; set; }
        public int PlexAccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime LastLogin { get; set; }
        public List<string> Roles { get; set; }
    }
}
