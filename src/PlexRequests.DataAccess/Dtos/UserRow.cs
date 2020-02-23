using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("Users", Schema = "Plex")]
    public class UserRow : TimestampRow
    {
        public UserRow()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            UserRoles = new List<UserRoleRow>();
            // ReSharper disable once VirtualMemberCallInConstructor
            UserRefreshTokens = new List<UserRefreshTokenRow>();
        }

        [Key]
        public int UserId { get; set; }
        public Guid Identifier { get; set; }
        public int PlexAccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime? LastLoginUtc { get; set; }
        public DateTime? InvalidateTokensBeforeUtc { get; set; }
        public virtual ICollection<UserRoleRow> UserRoles { get; set; }
        public virtual ICollection<UserRefreshTokenRow> UserRefreshTokens { get; set; }
    }
}
