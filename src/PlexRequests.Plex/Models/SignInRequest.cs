namespace PlexRequests.Plex.Models
{
    public class SignInRequest
    {
        public UserRequest User {get; set;} 
    }

    public class UserRequest {
        public string Login {get; set;}
        public string Password {get; set;}
    }
}