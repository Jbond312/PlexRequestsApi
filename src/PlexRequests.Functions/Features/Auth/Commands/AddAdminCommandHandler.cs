using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlexRequests.Core.Auth;
using PlexRequests.Core.ExtensionMethods;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;

namespace PlexRequests.Functions.Features.Auth.Commands
{
    public class AddAdminCommandHandler : IRequestHandler<AddAdminCommand, ValidationContext<UserLoginCommandResult>>
    {
        private readonly IUserService _userService;
        private readonly IPlexService _plexService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlexApi _plexApi;
        private readonly PlexSettings _plexSettings;
        private readonly ILogger<AddAdminCommandHandler> _logger;

        public AddAdminCommandHandler(
            IUserService userService,
            IPlexService plexService,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IPlexApi plexApi,
            IOptionsSnapshot<PlexSettings> plexSettings,
            ILogger<AddAdminCommandHandler> logger
            )
        {
            _userService = userService;
            _plexService = plexService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _plexApi = plexApi;
            _plexSettings = plexSettings.Value;
            _logger = logger;
        }

        public async Task<ValidationContext<UserLoginCommandResult>> Handle(AddAdminCommand request, CancellationToken cancellationToken)
        {
            var result =  new ValidationContext<UserLoginCommandResult>();
            _logger.LogDebug("Attempting to create first Admin account");

            if (await result.AddErrorIf(HasAdminAlreadyBeenCreated, "Unable to add Plex Admin", "An Admin account has already been created"))
            {
                return result;
            }

            _logger.LogDebug("No existing Admin account, attempting Plex SignIn");
            var plexUser = await _plexApi.SignIn(request.Username, request.Password);

            if (result.AddErrorIf(() => plexUser == null, "Invalid PlexCredentials", "The Login credentials for Plex were invalid"))
            {
                _logger.LogDebug("Invalid PlexCredentials");
                return result;
            }

            if (!result.IsSuccessful)
            {
                return result;
            }

            _logger.LogDebug("Plex SignIn Successful");

            var refreshToken = _tokenService.CreateRefreshToken();
            
            var adminUser = await AddAdminUser(plexUser, refreshToken);

            await CreateAdminServer(plexUser);

            adminUser.LastLoginUtc = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();
            
            result.Data = new UserLoginCommandResult
            {
                AccessToken = _tokenService.CreateToken(adminUser),
                RefreshToken = refreshToken.Token
            };

            return result;
        }

        private async Task CreateAdminServer(User plexUser)
        {
            _logger.LogDebug("Getting PlexServers for Admin user");
            var servers = await _plexApi.GetServers(plexUser.AuthToken);

            var adminServer = servers?.FirstOrDefault(x => x.Owned == "1");

            if (adminServer != null)
            {
                await CreateAdminServer(adminServer, plexUser);
            }
            else
            {
                _logger.LogInformation("No PlexServer found that is owned by the Admin account");
            }
        }

        private async Task<UserRow> AddAdminUser(User plexUser, UserRefreshTokenRow refreshToken)
        {
            var adminUser = new UserRow
            {
                Identifier = Guid.NewGuid(),
                Username = plexUser.Username,
                Email = plexUser.Email,
                PlexAccountId = plexUser.Id,
                IsAdmin = true,
                UserRoles = new List<UserRoleRow>
                {
                    new UserRoleRow
                    {
                        Role = PlexRequestRoles.Admin
                    },
                    new UserRoleRow
                    {
                        Role = PlexRequestRoles.User
                    },
                    new UserRoleRow
                    {
                        Role = PlexRequestRoles.Commenter
                    }
                },
                UserRefreshTokens = new List<UserRefreshTokenRow> {refreshToken}
            };

            _logger.LogInformation("Creating Admin account");
            await _userService.AddUser(adminUser);
            return adminUser;
        }

        private async Task<bool> HasAdminAlreadyBeenCreated()
        {
            var adminAlreadyExists = await _userService.IsAdminCreated();

            if (adminAlreadyExists)
            {
                _logger.LogInformation("Attempt to create Admin account when one already exists");
            }

            return adminAlreadyExists;
        }


        private async Task CreateAdminServer(Server adminServer, User plexUser)
        {
            _logger.LogInformation("Found a PlexServer owned by the Admin account");
            var plexServer = new PlexServerRow
            {
                Identifier = Guid.NewGuid(),
                AccessToken = adminServer.AccessToken,
                Name = adminServer.Name,
                MachineIdentifier = adminServer.MachineIdentifier,
                LocalIp = adminServer.LocalAddresses.Split(",").FirstOrDefault(),
                LocalPort = _plexSettings.DefaultLocalPort,
                ExternalIp = adminServer.Address,
                ExternalPort = Convert.ToInt32(adminServer.Port),
                Scheme = adminServer.Scheme,
                PlexLibraries = new List<PlexLibraryRow>()
            };

            _logger.LogInformation("Getting available libraries on PlexServer");

            var libraryContainer = await _plexApi.GetLibraries(plexUser.AuthToken,
                plexServer.GetPlexUri(_plexSettings.ConnectLocally));

            var directories = libraryContainer?.MediaContainer.Directory;

            if (directories != null)
            {
                _logger.LogInformation(
                    $"Identified '{directories.Count}' libraries on the PlexServer");

                plexServer.PlexLibraries = directories.Select(x =>
                    new PlexLibraryRow
                    {
                        LibraryKey = x.Key,
                        Title = x.Title,
                        Type = x.Type
                    }).ToList();
            }
            else
            {
                _logger.LogInformation("No Plex Libraries were found for the server");
            }

            await _plexService.AddServer(plexServer);
        }
    }
}
