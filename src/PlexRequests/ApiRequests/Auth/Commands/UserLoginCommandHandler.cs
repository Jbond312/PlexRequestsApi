using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.Plex;

namespace PlexRequests.ApiRequests.Auth.Commands
{
    public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, ValidationContext<UserLoginCommandResult>>
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlexApi _plexApi;
        private readonly ILogger<UserLoginCommandHandler> _logger;

        public UserLoginCommandHandler(
            IUserService userService,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IPlexApi plexApi,
            ILogger<UserLoginCommandHandler> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _plexApi = plexApi;
            _logger = logger;
        }
        public async Task<ValidationContext<UserLoginCommandResult>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Attempting Plex SignIn");

            var result = new ValidationContext<UserLoginCommandResult>();

            PlexRequests.Plex.Models.User plexUser;
            try
            {
                plexUser = await _plexApi.SignIn(request.Username, request.Password);
            }
            catch (PlexRequestException)
            {
                result.AddError("Invalid Plex Credentials", "Unable to login to Plex with the given credentials");
                _logger.LogInformation("Unable to login to Plex with the given credentials");
                return result;
            }

            _logger.LogDebug("Plex SignIn Successful");
            _logger.LogDebug("Getting PlexRequests User from PlexAccountId");

            var plexRequestsUser = await _userService.GetUserFromPlexId(plexUser.Id);

            if (plexRequestsUser == null || plexRequestsUser.IsDisabled)
            {
                result.AddError("Unrecognised user", "The user is not recognised or has been disabled.");
                _logger.LogInformation($"Successful login from a Plex account that was either not found within our users list or has been disabled [IsDisabled={plexRequestsUser?.IsDisabled}]");
                return result;
            }

            _logger.LogDebug("Found matching PlexRequests User");

            var refreshToken = _tokenService.CreateRefreshToken();
            var accessToken = _tokenService.CreateToken(plexRequestsUser);

            plexRequestsUser.LastLoginUtc = DateTime.UtcNow;
            plexRequestsUser.UserRefreshTokens.Add(refreshToken);

            await _unitOfWork.CommitAsync();

            result.Data = new UserLoginCommandResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };

            return result;
        }
    }
}
