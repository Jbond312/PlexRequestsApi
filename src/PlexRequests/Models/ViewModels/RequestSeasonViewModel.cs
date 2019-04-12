using System.Collections.Generic;

namespace PlexRequests.Models.ViewModels
{
    public class RequestSeasonViewModel
    {
        public int Season { get; set; }
        public List<RequestEpisodeViewModel> Episodes { get; set; }
    }
}