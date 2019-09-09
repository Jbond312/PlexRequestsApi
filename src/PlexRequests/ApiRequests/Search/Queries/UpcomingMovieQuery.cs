using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class UpcomingMovieQuery : IRequest<List<MovieSearchModel>>
    {
        public UpcomingMovieQuery(int? page)
        {
            Page = page;
        }

        public int? Page { get; set; }
    }
}
