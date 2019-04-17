using System.Collections.Generic;
using MediatR;
using PlexRequests.Models.SubModels.Create;

namespace PlexRequests.Models.Requests
{
    public class CreateTvRequestCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
        public List<TvRequestSeasonCreateModel> Seasons { get; set; }
    }
}