using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Requests.DTOs.Create;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateTvRequestCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
        public bool TrackShow { get; set; }
        public List<TvRequestSeasonCreateModel> Seasons { get; set; }
    }
}