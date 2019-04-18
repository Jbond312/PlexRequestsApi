using System.Collections.Generic;

namespace PlexRequests.Repository.Models
{
    public class PlexSeason: BasePlexMediaItem
    {
        public PlexSeason()
        {
            Episodes = new List<PlexEpisode>();
        }

        public int Season { get; set; }
        public List<PlexEpisode> Episodes { get; set; }
    }
}
