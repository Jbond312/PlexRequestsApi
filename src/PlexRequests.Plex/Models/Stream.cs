namespace PlexRequests.Plex.Models
{
    public class Stream
    {
        public int Id { get; set; }
        public int StreamType { get; set; }
        public bool Default { get; set; }
        public string Codec { get; set; }
        public int Index { get; set; }
        public int Bitrate { get; set; }
        public int BitDepth { get; set; }
        public string ChromaLocation { get; set; }
        public string ChromaSubsampling { get; set; }
        public string ColorPrimaries { get; set; }
        public string ColorRange { get; set; }
        public string ColorSpace { get; set; }
        public string ColorTrc { get; set; }
        public double FrameRate { get; set; }
        public bool HasScalingMatrix { get; set; }
        public int Height { get; set; }
        public int Level { get; set; }
        public string Profile { get; set; }
        public int RefFrames { get; set; }
        public string StreamIdentifier { get; set; }
        public int Width { get; set; }
        public string DisplayTitle { get; set; }
        public bool? Selected { get; set; }
        public int? Channels { get; set; }
        public string Language { get; set; }
        public string LanguageCode { get; set; }
        public int? SamplingRate { get; set; }
        public string AudioChannelLayout { get; set; }

        //TV Episode
        public bool Anamorphic { get; set; }
        public string PixelAspectRatio { get; set; }
    }
}