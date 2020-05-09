using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Functions.Features.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class PopularTvQuery : IRequest<List<TvSearchModel>>
    {
        public PopularTvQuery(int? page)
        {
            Page = page;
        }

        [Range(1, int.MaxValue)]
        public int? Page { get; set; }
    }
}
