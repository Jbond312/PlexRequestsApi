using MediatR;

namespace PlexRequests.ApiRequests.Auth.Commands
{
    public class RefreshTokenCommand : IRequest<UserLoginCommandResult>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
