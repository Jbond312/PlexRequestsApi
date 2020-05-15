using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;

namespace PlexRequests.Functions.Features.Auth.Commands
{
    public class RemoveExpiredRefreshTokensCommandHandler : IRequestHandler<RemoveExpiredRefreshTokensCommand, RemoveExpiredRefreshTokensCommandResult>
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveExpiredRefreshTokensCommandHandler> _logger;

        public RemoveExpiredRefreshTokensCommandHandler(
            IUserService userService,
            IUnitOfWork unitOfWork,
            ILogger<RemoveExpiredRefreshTokensCommandHandler> logger
            )
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RemoveExpiredRefreshTokensCommandResult> Handle(RemoveExpiredRefreshTokensCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Deleting expired tokens prior to [{DateTime.UtcNow}]");

            _userService.DeleteExpiredUserTokens();

            await _unitOfWork.CommitAsync();

            return new RemoveExpiredRefreshTokensCommandResult();
        }
    }
}
