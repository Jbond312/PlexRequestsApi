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
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly AuthenticationSettings _authSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IOptionsSnapshot<AuthenticationSettings> authSettings,
            ILogger<TokenService> logger)
        {
            _logger = logger;
            _authSettings = authSettings.Value;
        }

        public string CreateToken(UserRow user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role));
            }

            var expiry = DateTime.UtcNow.AddMinutes(_authSettings.TokenExpirationMinutes);
            //Remove milliseconds from the time as it gets stripped when the token is serialised
            expiry = expiry.AddMilliseconds(-expiry.Millisecond);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
                Audience = _authSettings.Audience,
                Issuer = _authSettings.Issuer
            };

            user.InvalidateTokensBeforeUtc = expiry;

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public UserRefreshTokenRow CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            return new UserRefreshTokenRow
            {
                Token = refreshToken,
                ExpiresUtc = DateTime.UtcNow.AddDays(_authSettings.RefreshTokenExpirationDays)
            };
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
                ValidIssuer = _authSettings.Issuer,
                ValidAudience = _authSettings.Audience,
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
