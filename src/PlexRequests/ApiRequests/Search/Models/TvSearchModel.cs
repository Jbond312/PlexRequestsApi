using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Search.Models
{
    public class TvSearchModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestStatuses? RequestStatus { get; set; }
        public string PlexMediaUri { get; set; }
    }
}
