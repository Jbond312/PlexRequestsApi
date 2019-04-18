namespace PlexRequests.Repository.Models
{
    public class PlexEpisode : BasePlexMediaItem
    {
        public int Episode { get; set; }
        public int? Year { get; set; }
        public string Resolution { get; set; }
    }
}