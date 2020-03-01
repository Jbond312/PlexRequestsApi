using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Search.Models
{
    public class MovieDetailModel
    {
        public int Id { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? Runtime { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestStatuses? RequestStatus { get; set; }
        public string PlexMediaUri { get; set; }
    }
}
