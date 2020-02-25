using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.ApiRequests.Requests.Models.Create;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateTvRequestCommand : IRequest<ValidationContext>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int TheMovieDbId { get; set; }
        public bool TrackShow { get; set; }
        public List<TvRequestSeasonCreateModel> Seasons { get; set; }
    }
}