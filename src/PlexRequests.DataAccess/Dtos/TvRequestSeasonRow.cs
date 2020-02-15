﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    public class TvRequestSeasonRow : TimestampRow
    {
        public TvRequestSeasonRow()
        {
            TvRequestEpisodes = new List<TvRequestEpisodeRow>();
        }

        [Key]
        public int TvRequestSeasonId { get; set; }
        [ForeignKey("TvRequestId")]
        public virtual TvRequestRow TvRequest { get; set; }
        public int TvRequestId { get; set; }
        [ForeignKey("PlexSeasonId")]
        public virtual PlexSeasonRow PlexSeason { get; set; }
        public int PlexSeasonId { get; set; }
        public string Title { get; set; }
        public int SeasonIndex { get; set; }
        [Column("RequestStatusId")]
        public RequestStatuses RequestStatus { get; set; }
        public string ImagePath { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public virtual ICollection<TvRequestEpisodeRow> TvRequestEpisodes { get; set; }
    }
}
