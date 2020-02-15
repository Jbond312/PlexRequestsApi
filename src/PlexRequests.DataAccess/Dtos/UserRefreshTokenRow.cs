using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("UserRefreshTokens", Schema = "Plex")]
    public class UserRefreshTokenRow : TimestampRow
    {
        [Key] 
        public int UserRefreshTokenId { get; set; }
        [ForeignKey("UserId")] 
        public virtual UserRow User { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresUtc { get; set; }
    }
}
