using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Plex;

namespace PlexRequests.ApiRequests.Auth.Commands
{
    public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, UserLoginCommandResult>
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IPlexApi _plexApi;
        private readonly ILogger<UserLoginCommandHandler> _logger;

        public UserLoginCommandHandler(
            IUserService userService,
            ITokenService tokenService,
            IPlexApi plexApi,
            ILogger<UserLoginCommandHandler> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _plexApi = plexApi;
            _logger = logger;
        }
        public async Task<UserLoginCommandResult> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Attempting Plex SignIn");

            var plexUser = await _plexApi.SignIn(request.Username, request.Password);

            if (plexUser == null)
            {
                _logger.LogDebug("Invalid PlexCredentials");
                throw new PlexRequestException("Invalid Plex Credentials", "Unable to login to Plex with the given credentials");
            }

            _logger.LogDebug("Plex SignIn Successful");

            _logger.LogDebug("Getting PlexRequests User from PlexAccountId");

            var plexRequestsUser = await _userService.GetUserFromPlexId(plexUser.Id);

            if (plexRequestsUser == null || plexRequestsUser.IsDisabled)
            {
                _logger.LogInformation("Attempted login by unknown user.");
                throw new PlexRequestException("Unrecognised user", "The user is not recognised or has been disabled.");
            }

            _logger.LogDebug("Found matching PlexRequests User");

            var refreshToken = _tokenService.CreateRefreshToken();
            var accessToken = _tokenService.CreateToken(plexRequestsUser);

            plexRequestsUser.LastLogin = DateTime.UtcNow;
            plexRequestsUser.RefreshToken = refreshToken;

            await _userService.UpdateUser(plexRequestsUser);

            var result = new UserLoginCommandResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };

            return result;
        }
    }
}
