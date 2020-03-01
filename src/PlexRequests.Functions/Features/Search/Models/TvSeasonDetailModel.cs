using System;
using System.Collections.Generic;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Search.Models
{
    public class TvSeasonDetailModel
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public DateTime? AirDate { get; set; }
        public RequestStatuses? RequestStatus { get; set; }
        public string PlexMediaUri { get; set; }
        public List<EpisodeModel> Episodes { get; set; }
    }
}
