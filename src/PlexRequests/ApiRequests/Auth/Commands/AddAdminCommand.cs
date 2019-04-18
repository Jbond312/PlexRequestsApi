using MediatR;

namespace PlexRequests.ApiRequests.Auth.Commands
{
    public class AddAdminCommand : IRequest<UserLoginCommandResult>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
