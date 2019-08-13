namespace PlexRequests.ApiRequests.TheMovieDb.DTOs
{
    public class EpisodeModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public int SeasonIndex { get; set; }
        public string StillPath { get; set; }
    }
}
