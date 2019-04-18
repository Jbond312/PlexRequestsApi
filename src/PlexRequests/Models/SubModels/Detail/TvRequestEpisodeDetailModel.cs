using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Models.SubModels.Detail
{
    public class TvRequestEpisodeDetailModel
    {
        public string Title { get; set; }
        public int Index { get; set; }
        public string PlexMediaUri { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestStatuses Status { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDate { get; set; }
    }
}