using System.Collections.Generic;

namespace PlexRequests.Models.SubModels.Create
{
    public class TvRequestSeasonCreateModel
    {
        public int Index { get; set; }
        public List<TvRequestEpisodeCreateModel> Episodes { get; set; }
    }
}