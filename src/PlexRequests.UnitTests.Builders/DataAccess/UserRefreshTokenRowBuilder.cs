using System;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class UserRefreshTokenRowBuilder : IBuilder<UserRefreshTokenRow>
    {
        private int _userRefreshTokenId;
        private DateTime _expiresUtc;
        private string _token;

        public UserRefreshTokenRowBuilder()
        {
            _userRefreshTokenId = 1;
            _expiresUtc = DateTime.UtcNow.AddHours(1);
            _token = Guid.NewGuid().ToString();
        }

        public UserRefreshTokenRowBuilder WithId(int id)
        {
            _userRefreshTokenId = id;
            return this;
        }

        public UserRefreshTokenRowBuilder WithExpiresUtc(DateTime expiresUtc)
        {
            _expiresUtc = expiresUtc;
            return this;
        }

        public UserRefreshTokenRow Build()
        {
            return new UserRefreshTokenRow
            {
                UserRefreshTokenId = _userRefreshTokenId,
                ExpiresUtc = _expiresUtc,
                Token = _token
            };
        }
    }
}
