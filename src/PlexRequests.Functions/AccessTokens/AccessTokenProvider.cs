using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlexRequests.Core.Settings;

namespace PlexRequests.Functions.AccessTokens
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string AuthHeaderName = "Authorization";
        private const string BearerPrefix = "Bearer ";

        private readonly AuthenticationSettings _authSettings;
        private readonly ILogger<AccessTokenProvider> _logger;
        
        public AccessTokenProvider(
            IOptions<AuthenticationSettings> authOptionsSnapshot,
            ILogger<AccessTokenProvider> logger
            )
        {
            _logger = logger;
            _authSettings = authOptionsSnapshot.Value;
        }

        public AccessTokenResult ValidateToken(HttpRequest request)
        {
            try
            {
                var authToken = GetAuthToken(request);
                if (string.IsNullOrEmpty(authToken))
                {
                    var tokenParams = new TokenValidationParameters()
                    {
                        RequireSignedTokens = true,
                        RequireExpirationTime = true,
                        ValidAudience = _authSettings.Audience,
                        ValidateAudience = true,
                        ValidIssuer = _authSettings.Issuer,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authSettings.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };

                    var handler = new JwtSecurityTokenHandler();
                    var result = handler.ValidateToken(authToken, tokenParams, out var securityToken);

                    if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.LogInformation("Unable to identify principal. Security token algorithms do not match");
                        return AccessTokenResult.Error(new Exception("Invalid security token"));
                    }

                    return AccessTokenResult.Success(result);
                }

                _logger.LogInformation("Request is unauthorised as no token was given in the request");

                return AccessTokenResult.NoToken();
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogInformation("Request is unauthorised as token has expired");
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Request is unauthorised due to unexpected exception");
                return AccessTokenResult.Error(ex);
            }
        }

        private static string GetAuthToken(HttpRequest request)
        {
            if (request != null &&
                request.Headers.ContainsKey(AuthHeaderName) &&
                request.Headers[AuthHeaderName].ToString().StartsWith(BearerPrefix))
            {
                return request.Headers[AuthHeaderName].ToString().Substring(BearerPrefix.Length);
            }

            return string.Empty;
        }
    }
}
