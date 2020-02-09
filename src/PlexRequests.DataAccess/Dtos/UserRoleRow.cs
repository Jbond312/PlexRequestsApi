using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("UserRoles", Schema = "Plex")]
    public class UserRoleRow : TimestampRow
    {
        [Key]
        public int UserRoleId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserRow User { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }
    }
}
