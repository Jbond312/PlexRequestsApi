using System;
using System.Collections.Generic;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.ApiRequests.Requests.Models.Detail
{
    public class TvRequestSeasonDetailModel
    {
        public int Index { get; set; }
        public string PlexMediaUri { get; set; }
        public List<TvRequestEpisodeDetailModel> Episodes { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDate { get; set; }
        public RequestStatuses Status { get; set; }
    }
}