using System.Collections.Generic;
using MediatR;

namespace PlexRequests.Models.Requests
{
    public class CreateTvRequestCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
        public Dictionary<int, List<int>> SeasonEpisodes { get; set; }
    }
}