using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class PopularMovieQuery : IRequest<List<MovieSearchModel>>
    {
        public PopularMovieQuery(int? page)
        {
            Page = page;
        }

        public int? Page { get; set; }
    }
}
