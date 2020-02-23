using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class SearchTvQuery : IRequest<List<TvSearchModel>>
    {
        public SearchTvQuery(string query, int? page)
        {
            Query = query;
            Page = page;
        }

        public int? Page { get; set; }
        public string Query { get; set; }
    }
}