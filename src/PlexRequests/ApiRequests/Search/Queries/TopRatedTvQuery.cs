using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class TopRatedTvQuery : IRequest<List<TvSearchModel>>
    {
        public TopRatedTvQuery(int? page)
        {
            Page = page;
        }

        public int? Page { get; set; }
    }
}
