using System.ComponentModel.DataAnnotations;

namespace PlexRequests.Functions.Features.Requests.Models.Create
{
    public class TvRequestEpisodeCreateModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Index { get; set; }
    }
}