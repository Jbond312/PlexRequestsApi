using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IPlexApi _plexApi;
        private readonly AuthenticationSettings _authSettings;

        public AuthController(IUserService userService, IPlexApi plexApi, IOptions<AuthenticationSettings> authSettings)
        {
            _userService = userService;
            _plexApi = plexApi;
            _authSettings = authSettings.Value;
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var plexUser = await _plexApi.SignIn(request.Username, request.Password);

            if (plexUser == null)
            {
                return BadRequest("Unable to login to Plex with the given credentials");
            }

            var plexRequestsUser = await _userService.GetUserFromPlexId(plexUser.Id);

            if (plexRequestsUser == null)
            {
                return Unauthorized();
            }

            plexRequestsUser.LastLogin = DateTime.UtcNow;

            await _userService.UpdateUser(plexRequestsUser);

            var result = new UserLoginResult
            {
                AccessToken = CreateToken(plexRequestsUser)
            };

            return Ok(result);
        }

        [HttpPost]
        [Route("CreateAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> AddPlexAdmin([FromQuery] [Required] string username,
            [FromQuery] [Required] string password)
        {
            if (await _userService.IsAdminCreated())
            {
                return BadRequest("An Admin account has already been created");
            }

            var plexUser = await _plexApi.SignIn(username, password);

            if (plexUser == null)
            {
                return BadRequest("Unable to login to Plex with the given credentials");
            }

            var adminUser = new User
            {
                Username = plexUser.Username,
                Email = plexUser.Email,
                PlexAccountId = plexUser.Id,
                IsAdmin = true,
                Roles = new List<string> { PlexRequestRoles.Admin, PlexRequestRoles.User }
            };

            await _userService.CreateUser(adminUser);

            var result = new CreateAdminResult
            {
                AccessToken = CreateToken(adminUser)
            };

            return Ok(result);
        }

        private string CreateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
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
