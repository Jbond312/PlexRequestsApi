using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Search.Models
{
    public class MovieSearchModel
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
