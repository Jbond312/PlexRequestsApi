using System.Collections.Generic;

namespace PlexRequests.Plex.Models
{
    public class Part
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Duration { get; set; }
        public string File { get; set; }
        public long Size { get; set; }
        public string Container { get; set; }
        public bool Has64BitOffsets { get; set; }
        public bool OptimizedForStreaming { get; set; }
        public string VideoProfile { get; set; }
        public List<Stream> Stream { get; set; }

        //TV Show Episode
        public string AudioProfile { get; set; }

        //Movie Section
        public string HasThumbnail { get; set; }
        public string Indexes { get; set; }
        public bool? HasChapterTextStream { get; set; }
    }
}