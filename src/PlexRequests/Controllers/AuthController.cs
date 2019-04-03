using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models;
using PlexRequests.Plex;
using PlexRequests.Settings;
using PlexRequests.Store.Models;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPlexService _plexService;
        private readonly IPlexApi _plexApi;
        private readonly AuthenticationSettings _authSettings;
        private readonly PlexSettings _plexSettings;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IPlexService plexService,
            IPlexApi plexApi,
            IOptions<AuthenticationSettings> authSettings,
            IOptions<PlexSettings> plexSettings,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _plexService = plexService;
            _plexApi = plexApi;
            _authSettings = authSettings.Value;
            _plexSettings = plexSettings.Value;
            _logger = logger;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserLoginResult>> Login(UserLoginRequest request)
        {
            _logger.LogDebug("Attempting Plex SignIn");

            var plexUser = await _plexApi.SignIn(request.Username, request.Password);

            if (plexUser == null)
            {
                _logger.LogDebug("Invalid PlexCredentials");
                return BadRequest("Unable to login to Plex with the given credentials");
            }

            _logger.LogDebug("Plex SignIn Successful");

            _logger.LogDebug("Getting PlexRequests User from PlexAccountId");

            var plexRequestsUser = await _userService.GetUserFromPlexId(plexUser.Id);

            if (plexRequestsUser == null || plexRequestsUser.IsDisabled)
            {
                _logger.LogInformation("Attempted login by unknown user.");
                return Unauthorized();
            }

            _logger.LogDebug("Found matching PlexRequests User");

            plexRequestsUser.LastLogin = DateTime.UtcNow;

            await _userService.UpdateUser(plexRequestsUser);

            var result = new UserLoginResult
            {
                AccessToken = CreateToken(plexRequestsUser)
            };

            return result;
        }

        [HttpPost("CreateAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<UserLoginResult>> AddPlexAdmin(UserLoginRequest request)
        {
            _logger.LogDebug("Attempting to create first Admin account");

            if (await _userService.IsAdminCreated())
            {
                _logger.LogInformation("Attempt to create Admin account when one already exists");
                throw new PlexRequestException("Unable to add Plex Admin", "An Admin account has already been created");
            }

            _logger.LogDebug("No existing Admin account, attempting Plex SignIn");
            var plexUser = await _plexApi.SignIn(request.Username, request.Password);

            if (plexUser == null)
            {
                _logger.LogDebug("Invalid PlexCredentials");
                return BadRequest("Unable to login to Plex with the given credentials");
            }

            _logger.LogDebug("Plex SignIn Successful");

            var adminUser = new User
            {
                Username = plexUser.Username,
                Email = plexUser.Email,
                PlexAccountId = plexUser.Id,
                IsAdmin = true,
                Roles = new List<string> { PlexRequestRoles.Admin, PlexRequestRoles.User }
            };

            _logger.LogInformation("Creating Admin account");
            await _userService.CreateUser(adminUser);
            
            _logger.LogDebug("Getting PlexServers");
            var servers = await _plexApi.GetServers(plexUser.AuthToken);

            var adminServer = servers.FirstOrDefault(x => x.Owned == "1");

            if (adminServer != null)
            {
                _logger.LogInformation("Found a PlexServer owned by the Admin account");
                var plexServer = new PlexServer
                {
                    AccessToken = adminServer.AccessToken,
                    Name = adminServer.Name,
                    MachineIdentifier = adminServer.MachineIdentifier,
                    LocalIp = adminServer.LocalAddresses.Split(",").FirstOrDefault(),
                    LocalPort = _plexSettings.DefaultLocalPort,
                    ExternalIp =  adminServer.Address,
                    ExternalPort = Convert.ToInt32(adminServer.Port),
                    Scheme = adminServer.Scheme
                };

                _logger.LogInformation("Getting available libraries on PlexServer");
                var libraryContainer = await _plexApi.GetLibraries(plexUser.AuthToken,
                    plexServer.GetPlexUri(_plexSettings.ConnectLocally));
                _logger.LogInformation($"Identified '{libraryContainer.MediaContainer.Directory.Count}' libraries on the PlexServer");

                plexServer.Libraries = libraryContainer.MediaContainer.Directory.Select(x =>
                    new PlexServerLibrary
                    {
                        Key = x.Key,
                        Title = x.Title,
                        Type = x.Type
                    }).ToList();

                await _plexService.Create(plexServer);
            }
            else
            {
                _logger.LogInformation("No PlexServer found that is owned by the Admin account");
            }

            var result = new UserLoginResult
            {
                AccessToken = CreateToken(adminUser)
            };

            return result;
        }

        private string CreateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new  ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
                Audience = "PlexRequests",
                Issuer = "PlexRequests"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
