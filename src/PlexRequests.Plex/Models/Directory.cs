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
    }
}
