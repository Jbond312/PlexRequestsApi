using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Functions.Features.Auth.Commands
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ValidationContext<UserLoginCommandResult>>
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            ITokenService tokenService,
            IUserService userService,
            IUnitOfWork unitOfWork,
            ILogger<RefreshTokenCommandHandler> logger
            )
        {
            _tokenService = tokenService;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ValidationContext<UserLoginCommandResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Attempting to refresh users token");

            var resultContext = new ValidationContext<UserLoginCommandResult>();

            if (string.IsNullOrWhiteSpace(request.RefreshToken) || string.IsNullOrWhiteSpace(request.AccessToken))
            {
                resultContext
                .AddError("Invalid Token");
                _logger.LogDebug("RefreshToken or AccessToken was not specified when attempting to refresh a token");
                return resultContext;
            }

            var principal = _tokenService.GetPrincipalFromAccessToken(request.AccessToken);
            var userIdClaim = principal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            var user = await ValidateAndReturnUser(userIdClaim, resultContext);
            
            if (!IsUserRefreshTokenValid(user?.UserRefreshTokens, request.RefreshToken))
            {
                resultContext.AddError("Invalid Token");
                _logger.LogDebug("Refresh token has either expired or does not match the users current refresh token");
            }

            if (!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            _logger.LogDebug("Refresh token is valid, re-creating tokens for user.");
            
            var accessToken = _tokenService.CreateToken(user);
            var refreshToken = _tokenService.CreateRefreshToken();
            // ReSharper disable once PossibleNullReferenceException
            user.UserRefreshTokens.Add(refreshToken);

            await _unitOfWork.CommitAsync();

            resultContext.Data = new UserLoginCommandResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };

            return resultContext;
        }

        private async Task<UserRow> ValidateAndReturnUser(Claim userIdClaim, ValidationContext<UserLoginCommandResult> context)
        {
            if (userIdClaim?.Value == null)
            {
                context.AddError("Invalid Token");
                _logger.LogDebug("UserId could not be extracted from the claim");
                return null;
            }

            var user = await _userService.GetUser(Guid.Parse(userIdClaim.Value));

            if (user != null && !user.IsDisabled)
            {
                return user;
            }

            context.AddError("Invalid Token");
            _logger.LogDebug("User either no longer exists or has been disabled");
            return null;

        }

        private static bool IsUserRefreshTokenValid(ICollection<UserRefreshTokenRow> userRefreshTokens, string refreshTokenToValidate)
        {
            return userRefreshTokens.Any() && userRefreshTokens.Any(x => DateTime.UtcNow <= x.ExpiresUtc && x.Token.Equals(refreshTokenToValidate, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
