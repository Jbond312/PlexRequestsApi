using System;
using System.Collections.Generic;

namespace PlexRequests.ApiRequests.TheMovieDb.Models
{
    public class TvSeasonDetailModel
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public DateTime? AirDate { get; set; }
        public List<EpisodeModel> Episodes { get; set; }
    }
}
