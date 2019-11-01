using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class TopRatedMovieQuery : IRequest<List<MovieSearchModel>>
    {
        public TopRatedMovieQuery(int? page)
        {
            Page = page;
        }

        public int? Page { get; set; }
    }
}
