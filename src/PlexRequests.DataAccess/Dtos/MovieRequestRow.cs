﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("MovieRequests", Schema = "Plex")]
    public class MovieRequestRow : TimestampRow
    {
        public MovieRequestRow()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            MovieRequestAgents = new List<MovieRequestAgentRow>();
            RequestStatus = RequestStatuses.PendingApproval;
        }

        [Key]
        public int MovieRequestId { get; set; }
        public int TheMovieDbId { get; set; }
        [ForeignKey("PlexMediaItemId")]
        public virtual PlexMediaItemRow PlexMediaItem { get; set; }
        public int? PlexMediaItemId { get; set; }
        public string Title { get; set; }
        [ForeignKey("UserId")] 
        public virtual UserRow User { get; set; }
        public int UserId { get; set; }
        [Column("RequestStatusId")]
        public RequestStatuses RequestStatus { get; set; }
        public string ImagePath { get; set; }
        public string Comment { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public virtual ICollection<MovieRequestAgentRow> MovieRequestAgents { get; set; }
        public MovieRequestAgentRow PrimaryAgent => MovieRequestAgents.FirstOrDefault(x => x.AgentType == AgentTypes.TheMovieDb);
    }
}
