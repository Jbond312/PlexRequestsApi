using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("TvRequestUsers", Schema = "Plex")]
    public class TvRequestUserRow : TimestampRow
    {
        [Key]
        public int TvRequestUserId { get; set; }
        [ForeignKey("TvRequestId")]
        public virtual TvRequestRow TvRequest { get; set; }
        public int TvRequestId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserRow User { get; set; }
        public int UserId { get; set; }
        public int? Season { get; set; }
        public int? Episode { get; set; }
        public bool Track { get; set; }
    }
}
