using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("PlexEpisodes", Schema = "Plex")]
    public class PlexEpisodeRow : TimestampRow
    {
        [Key]
        public int PlexEpisodeId { get; set; }
        public Guid Identifier { get; set; }
        [ForeignKey("PlexSeasonId")]
        public virtual PlexSeasonRow PlexSeason { get; set; }
        public int PlexSeasonId { get; set; }
        public int Episode { get; set; }
        public int MediaItemKey { get; set; }
        public string Title { get; set; }
        [Column("AgentTypeId")]
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
        public string MediaUri { get; set; }
        public int? Year { get; set; }
        public string Resolution { get; set; }
        public virtual ICollection<TvRequestEpisodeRow> TvRequestEpisodes { get; set; }
    }
}
