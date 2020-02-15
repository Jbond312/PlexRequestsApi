using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("PlexMediaItems", Schema = "Plex")]
    public class PlexMediaItemRow : TimestampRow
    {
        [Key]
        public int PlexMediaItemId { get; set; }
        public Guid Identifier { get; set; }
        [ForeignKey("PlexLibraryId")]
        public virtual PlexLibraryRow PlexLibrary { get; set; }
        public int PlexLibraryId { get; set; }
        public int MediaItemKey { get; set; }
        public string Title { get; set; }
        [Column("AgentTypeId")]
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
        public string MediaUri { get; set; }
        public int? Year { get; set; }
        public string Resolution { get; set; }
        [Column("MediaTypeId")]
        public PlexMediaTypes MediaType { get; set; }
        public virtual ICollection<PlexSeasonRow> PlexSeasons { get; set; }
    }
}
