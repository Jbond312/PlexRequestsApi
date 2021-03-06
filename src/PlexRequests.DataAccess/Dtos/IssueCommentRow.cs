﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("IssueComments", Schema = "Plex")]
    public class IssueCommentRow : TimestampRow
    {
        [Key]
        public int IssueCommentId { get; set; }
        [ForeignKey("IssueId")]
        public virtual IssueRow Issue { get; set; }
        public int IssueId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserRow User { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public int LikeCount { get; set; }
    }
}
