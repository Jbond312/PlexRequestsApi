using System.Collections.Generic;
using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class PopularTvQuery : IRequest<List<TvSearchModel>>
    {
        public PopularTvQuery(int? page)
        {
            Page = page;
        }

        public int? Page { get; set; }
    }
}
