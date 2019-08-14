using System.Collections.Generic;

namespace PlexRequests.ApiRequests.Requests.Models.Create
{
    public class TvRequestSeasonCreateModel
    {
        public int Index { get; set; }
        public List<TvRequestEpisodeCreateModel> Episodes { get; set; }
    }
}