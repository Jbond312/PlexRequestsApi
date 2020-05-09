using MediatR;

namespace PlexRequests.Functions.Features.Auth.Commands
{
    public class RefreshTokenCommand : IRequest<ValidationContext<UserLoginCommandResult>>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
