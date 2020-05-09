using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Functions.Features.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class GetTvDetailsQuery : IRequest<TvDetailModel>
    {
        public GetTvDetailsQuery(int tvId)
        {
            TvId = tvId;
        }

        [Required]
        [Range(1, int.MaxValue)]
        public int TvId { get; set; }
    }
}