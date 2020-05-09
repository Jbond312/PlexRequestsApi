using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Functions.Features.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class SearchMovieQuery : IRequest<List<MovieSearchModel>>
    {       
        [Required]
        [MinLength(1)]
        public string Query { get; set; }

        [Range(1753, int.MaxValue)]
        public int? Year { get; set; }

        [Range(1, int.MaxValue)]
        public int? Page { get; set; }

        public SearchMovieQuery(string query, int? year, int? page)
        {
            Query = query;
            Year = year;
            Page = page;
        }
    }
}
