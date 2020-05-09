using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Functions.Features.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class TopRatedMovieQuery : IRequest<List<MovieSearchModel>>
    {
        public TopRatedMovieQuery(int? page)
        {
            Page = page;
        }

        [Range(1, int.MaxValue)]
        public int? Page { get; set; }
    }
}
