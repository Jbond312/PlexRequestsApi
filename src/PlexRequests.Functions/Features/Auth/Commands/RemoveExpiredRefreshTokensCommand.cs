using MediatR;

namespace PlexRequests.Functions.Features.Auth.Commands
{
    public class RemoveExpiredRefreshTokensCommand : IRequest<RemoveExpiredRefreshTokensCommandResult>
    {
    }
}
