using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlexRequests.Core.Settings;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private const int AccessTokenLifespanMinutes = 10;
        private const int RefreshTokenLifespanDays = 1;

        private readonly AuthenticationSettings _authSettings;

        public TokenService(
            IOptions<AuthenticationSettings> authSettings,
            ILogger<TokenService> logger)
        {
            _logger = logger;
            _authSettings = authSettings.Value;
        }

        public string CreateToken(User user)
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

            var expiry = DateTime.UtcNow.AddMinutes(AccessTokenLifespanMinutes);
            //Remove milliseconds from the time as it gets stripped when the token is serialised
            expiry = expiry.AddMilliseconds(-expiry.Millisecond);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
                Audience = "PlexRequests",
                Issuer = "PlexRequests"
            };

            user.InvalidateTokensBefore = expiry;

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);
                return new RefreshToken(refreshToken, DateTime.UtcNow.AddDays(RefreshTokenLifespanDays));
            }
        }

        public ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken)
        {
            var tokenValidationParamers = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authSettings.SecretKey)),
                RequireExpirationTime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "PlexRequests",
                ValidAudience = "PlexRequests",
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(accessToken, tokenValidationParamers, out securityToken);
            }
            catch (SecurityTokenException securityTokenException)
            {
                _logger.LogDebug(securityTokenException.ToString());
                return null;
            }

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
    }
}
