using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlexRequests.Functions.Features.Requests.Models.Create
{
    public class TvRequestSeasonCreateModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Index { get; set; }
        public List<TvRequestEpisodeCreateModel> Episodes { get; set; }
    }
}