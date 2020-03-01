using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Functions.Features.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class SearchTvQuery : IRequest<List<TvSearchModel>>
    {
        public SearchTvQuery(string query, int? page)
        {
            Query = query;
            Page = page;
        }

        [Range(1, int.MaxValue)]
        public int? Page { get; set; }

        [Required]
        [MinLength(1)]
        public string Query { get; set; }
    }
}