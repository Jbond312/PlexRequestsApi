using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    public class IssueRow : TimestampRow
    {
        public IssueRow()
        {
            IssueComments = new List<IssueCommentRow>();
        }

        [Key]
        public int IssueId { get; set; }
        [ForeignKey("PlexMediaItemId")]
        public virtual PlexMediaItemRow PlexMediaItem { get; set; }
        public int PlexMediaItemId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserRow User { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IssueStatuses IssueStatus { get; set; }
        public virtual  ICollection<IssueCommentRow> IssueComments { get; set; }
    }
}
