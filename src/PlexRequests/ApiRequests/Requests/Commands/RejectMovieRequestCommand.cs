using System;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class RejectMovieRequestCommand : IRequest
    {
        [JsonIgnore]
        public Guid RequestId { get; set; }
        public string Comment { get; set; }
        
        public RejectMovieRequestCommand(Guid id, string comment)
        {
            RequestId = id;
            Comment = comment;
        }
    }
}