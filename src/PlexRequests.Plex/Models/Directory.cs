using System.Collections.Generic;

namespace PlexRequests.Plex.Models
{
    public class Directory
    {
        public int LeafCount { get; set; }
        public string Thumb { get; set; }
        public int ViewedLeafCount { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public bool AllowSync { get; set; }
        public string Art { get; set; }
        public string Composite { get; set; }
        public bool Filters { get; set; }
        public bool Refreshing { get; set; }
        public string Type { get; set; }
        public string Agent { get; set; }
        public string Scanner { get; set; }
        public string Language { get; set; }
        public string Uuid { get; set; }
        public int UpdatedAt { get; set; }
        public int CreatedAt { get; set; }
        public int ScannedAt { get; set; }
        public List<Location> Location { get; set; }
    }
}
