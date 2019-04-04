using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Plex;

namespace PlexRequests.Models.Auth
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
                throw new PlexRequestException("Unable to login to Plex with the given credentials");
            }

            _logger.LogDebug("Plex SignIn Successful");

            _logger.LogDebug("Getting PlexRequests User from PlexAccountId");

            var plexRequestsUser = await _userService.GetUserFromPlexId(plexUser.Id);

            if (plexRequestsUser == null || plexRequestsUser.IsDisabled)
            {
                _logger.LogInformation("Attempted login by unknown user.");
                return null;
            }

            _logger.LogDebug("Found matching PlexRequests User");

            plexRequestsUser.LastLogin = DateTime.UtcNow;

            await _userService.UpdateUser(plexRequestsUser);

            var result = new UserLoginCommandResult
            {
                AccessToken = _tokenService.CreateToken(plexRequestsUser)
            };

            return result;
        }
    }
}
