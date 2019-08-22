using System;

namespace PlexRequests.Repository.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }

        public RefreshToken()
        {
        }

        public RefreshToken(string token, DateTime expires)
        {
            Token = token;
            Expires = expires;
        }
    }
}
