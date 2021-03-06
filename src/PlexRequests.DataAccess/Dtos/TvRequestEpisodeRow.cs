﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("TvRequestEpisodes", Schema = "Plex")]
    public class TvRequestEpisodeRow : TimestampRow
    {
        public TvRequestEpisodeRow()
        {
            RequestStatus = RequestStatuses.PendingApproval;
        }

        [Key]
        public int TvRequestEpisodeId { get; set; }
        [ForeignKey("TvRequestSeasonId")]
        public virtual TvRequestSeasonRow TvRequestSeason { get; set; }
        public int TvRequestSeasonId { get; set; }
        [ForeignKey("PlexEpisodeId")]
        public virtual PlexEpisodeRow PlexEpisode { get; set; }
        public int PlexEpisodeId { get; set; }
        public string Title { get; set; }
        public int EpisodeIndex { get; set; }
        [Column("RequestStatusId")]
        public RequestStatuses RequestStatus { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDateUtc { get; set; }
    }
}
