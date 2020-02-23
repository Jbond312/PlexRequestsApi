using System.Collections.Generic;

namespace PlexRequests.Plex.Models
{
    public class Medium
    {
        //Movie
        public string VideoResolution { get; set; }
        public int Id { get; set; }
        public int Duration { get; set; }
        public int Bitrate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double AspectRatio { get; set; }
        public int AudioChannels { get; set; }
        public string AudioCodec { get; set; }
        public string VideoCodec { get; set; }
        public string Container { get; set; }
        public string VideoFrameRate { get; set; }
        public int OptimizedForStreaming { get; set; }
        public bool Has64BitOffsets { get; set; }
        public string VideoProfile { get; set; }
        public List<Part> Part { get; set; }

        //TV Show Episode
        public string AudioProfile { get; set; }
    }
}