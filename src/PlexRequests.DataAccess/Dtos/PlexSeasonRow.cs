using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("PlexSeasons", Schema = "Plex")]
    public class PlexSeasonRow :TimestampRow
    {
        public PlexSeasonRow()
        {
            TvRequestSeasons = new List<TvRequestSeasonRow>();
            PlexEpisodes = new List<PlexEpisodeRow>();
        }

        [Key]
        public int PlexSeasonId { get; set; }
        public Guid Identifier { get; set; }
        [ForeignKey("PlexMediaItemId")]
        public virtual PlexMediaItemRow PlexMediaItem { get; set; }
        public int PlexMediaItemId { get; set; }
        public int Season { get; set; }
        public int MediaItemKey { get; set; }
        public string Title { get; set; }
        [Column("AgentTypeId")]
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
        public string MediaUri { get; set; }
        public virtual ICollection<TvRequestSeasonRow> TvRequestSeasons { get; set; }
        public virtual ICollection<PlexEpisodeRow> PlexEpisodes { get; set; }
    }
}
