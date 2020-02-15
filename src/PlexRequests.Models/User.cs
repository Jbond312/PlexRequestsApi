using System;

namespace PlexRequests.Models
{
    public class User
    {
        public int UserId { get; set; }
        public Guid Identifier { get; set; }
        public int PlexAccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime? LastLoginUtc { get; set; }
        public DateTime? InvalidateTokensBeforeUtc { get; set; }

        public User(
            int userId,
            Guid identifier,
            int plexAccountId,
            string userName,
            string email,
            bool isAdmin = false,
            bool isDisabled = false,
            DateTime? lastLoginUtc = null,
            DateTime? invalidateTokensBeforeUtc = null
            )
        {
            UserId = userId;
            Identifier = identifier; 
            PlexAccountId = plexAccountId;
            Username = userName;
            Email = email;
            IsAdmin = isAdmin;
            IsDisabled = isDisabled;
            LastLoginUtc = lastLoginUtc;
            InvalidateTokensBeforeUtc = invalidateTokensBeforeUtc;
        }
    }
}
