using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class SearchMovieQuery : IRequest<List<MovieSearchModel>>
    {       
        public string Query { get; set; }
        public int? Year { get; set; }
        public int? Page { get; set; }

        public SearchMovieQuery(string query, int? year, int? page)
        {
            Query = query;
            Year = year;
            Page = page;
        }
    }
}
