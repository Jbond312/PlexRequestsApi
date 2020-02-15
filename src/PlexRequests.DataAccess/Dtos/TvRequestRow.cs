using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("TvRequests", Schema = "Plex")]
    public class TvRequestRow : TimestampRow
    {
        public TvRequestRow()
        {
            TvRequestSeasons = new List<TvRequestSeasonRow>();
            TvRequestAgents = new List<TvRequestAgentRow>();
            TvRequestUsers = new List<TvRequestUserRow>();
            RequestStatus = RequestStatuses.PendingApproval;
        }

        [Key]
        public int TvRequestId { get; set; }
        public int TheMovieDbId { get; set; }
        [ForeignKey("PlexMediaItemId")]
        public virtual PlexMediaItemRow PlexMediaItem { get; set; }
        public int? PlexMediaItemId { get; set; }
        public string Title { get; set; }
        [Column("RequestStatusId")]
        public RequestStatuses RequestStatus { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public bool Track { get; set; }
        public string Comment { get; set; }
        public virtual ICollection<TvRequestSeasonRow> TvRequestSeasons { get; set; }
        public virtual ICollection<TvRequestUserRow> TvRequestUsers { get; set; }
        public virtual ICollection<TvRequestAgentRow> TvRequestAgents { get; set; }
        public TvRequestAgentRow PrimaryAgent => TvRequestAgents.FirstOrDefault(x => x.AgentType == AgentTypes.TheMovieDb);
    }
}
