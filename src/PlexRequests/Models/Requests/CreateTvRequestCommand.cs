using System.Collections.Generic;
using MediatR;
using PlexRequests.Models.ViewModels;

namespace PlexRequests.Models.Requests
{
    public class CreateTvRequestCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
        public List<RequestSeasonViewModel> Seasons { get; set; }
    }
}