using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace PlexRequests.Plex.Models
{
    public class SignInAccount
    {
        public User User { get; set; }
    }

    public class User
    {
        public string Id { get; set; }
        public string Uuid { get; set; }
        public string Email { get; set; }
        public string Joined_At { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public string Thumb { get; set; }
        public bool HasPassword { get; set; }
        public string AuthToken { get; set; }
        public string Authentication_Token { get; set; }
        public Subscription Subscription { get; set; }
        public UserRole Roles { get; set; }
        public List<string> Entitlements { get; set; }
        public string ConfirmedAt { get; set; }
        public int? ForumId { get; set; }
        public bool RememberMe { get; set; }
    }
}