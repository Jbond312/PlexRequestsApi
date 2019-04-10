using System.Collections.Generic;
using MediatR;
using PlexRequests.Store.Models;

namespace PlexRequests.Models.Requests
{
    public class CreateTvRequestCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
        public List<RequestSeason> Seasons { get; set; }
    }
}