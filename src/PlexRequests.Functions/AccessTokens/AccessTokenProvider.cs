using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
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

        public AccessTokenProvider(IOptions<AuthenticationSettings> authOptionsSnapshot)
        {
            _authSettings = authOptionsSnapshot.Value;
        }

        public AccessTokenResult ValidateToken(HttpRequest request)
        {
            try
            {
                // Get the token from the header
                if (request != null &&
                    request.Headers.ContainsKey(AuthHeaderName) &&
                    request.Headers[AuthHeaderName].ToString().StartsWith(BearerPrefix))
                {
                    var token = request.Headers[AuthHeaderName].ToString().Substring(BearerPrefix.Length);

                    // Create the parameters
                    var tokenParams = new TokenValidationParameters()
                    {
                        RequireSignedTokens = true,
                        ValidAudience = _authSettings.Audience,
                        ValidateAudience = true,
                        ValidIssuer = _authSettings.Issuer,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authSettings.SecretKey))
                    };

                    // Validate the token
                    var handler = new JwtSecurityTokenHandler();
                    var result = handler.ValidateToken(token, tokenParams, out var securityToken);
                    return AccessTokenResult.Success(result);
                }
                else
                {
                    return AccessTokenResult.NoToken();
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                return AccessTokenResult.Error(ex);
            }
        }
    }
}
