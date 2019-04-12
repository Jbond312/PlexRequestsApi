using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRequests.Store.Enums;

namespace PlexRequests.Models.ViewModels
{
    public class RequestViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PlexMediaTypes MediaType { get; set; }
        public int? PlexRatingKey { get; set; }
        public string RequestedByUserName { get; set; }
        public List<RequestSeasonViewModel> Seasons { get; set; }
        public bool IsApproved { get; set; }
        public string ImagePath { get; set; }
        public DateTime AirDate { get; set; }
        public DateTime Created { get; set; }
    }
}