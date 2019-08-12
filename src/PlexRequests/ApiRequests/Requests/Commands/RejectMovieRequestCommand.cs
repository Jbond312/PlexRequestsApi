using System;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class RejectMovieRequestCommand : IRequest
    {
        [JsonIgnore]
        public Guid RequestId { get; set; }
        [Required]
        [MinLength(1)]
        public string Comment { get; set; }

        public RejectMovieRequestCommand(Guid id, string comment)
        {
            RequestId = id;
            Comment = comment;
        }
    }
}