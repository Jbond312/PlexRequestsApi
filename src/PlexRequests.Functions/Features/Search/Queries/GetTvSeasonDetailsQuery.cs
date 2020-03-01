using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Functions.Features.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class GetTvSeasonDetailsQuery : IRequest<TvSeasonDetailModel>
    {
        public GetTvSeasonDetailsQuery(int tvId, int seasonNumber)
        {
            TvId = tvId;
            SeasonNumber = seasonNumber;
        }

        [Required]
        [Range(1, int.MaxValue)]
        public int TvId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SeasonNumber { get; set; }
    }
}