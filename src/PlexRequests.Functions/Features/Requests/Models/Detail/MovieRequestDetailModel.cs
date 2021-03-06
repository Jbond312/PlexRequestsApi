using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Requests.Models.Detail
{
    public class MovieRequestDetailModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PlexMediaUri { get; set; }
        public string RequestedByUserName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestStatuses Status { get; set; }
        public string ImagePath { get; set; }
        public DateTime AirDate { get; set; }
        public DateTime Created { get; set; }
        public string Comment { get; set; }
    }
}