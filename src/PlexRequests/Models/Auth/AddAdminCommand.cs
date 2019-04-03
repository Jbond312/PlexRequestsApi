using MediatR;

namespace PlexRequests.Models.Auth
{
    public class AddAdminCommand : IRequest<UserLoginCommandResult>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
