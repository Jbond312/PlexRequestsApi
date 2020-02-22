using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class UserRowBuilder : IBuilder<UserRow>
    {
        private int _userId;
        private Guid _identifier;
        private int _plexAccountId;
        private string _userName;
        private string _email;
        private bool _isAdmin;
        private bool _isDisabled;
        private DateTime? _lastLoginUtc;
        private DateTime? _invalidateTokensBeforeUtc;
        private List<UserRoleRowBuilder> _userRoleBuilders;
        private List<UserRefreshTokenRowBuilder> _userRefreshTokenRowBuilders;

        public UserRowBuilder()
        {
            _userId = 1;
            _identifier = Guid.NewGuid();
            _plexAccountId = 1;
            _userName = Guid.NewGuid().ToString();
            _email = Guid.NewGuid().ToString();
            _isAdmin = false;
            _isDisabled = false;
            _lastLoginUtc = null;
            _invalidateTokensBeforeUtc = null;
            _userRoleBuilders = new List<UserRoleRowBuilder>();
            _userRefreshTokenRowBuilders = new List<UserRefreshTokenRowBuilder>();
        }

        public UserRowBuilder WithUserId(int id)
        {
            _userId = id;
            return this;
        }

        public UserRowBuilder WithUsername(string userName)
        {
            _userName = userName;
            return this;
        }

        public UserRowBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserRowBuilder WithIsAdmin(bool isAdmin)
        {
            _isAdmin = isAdmin;
            return this;
        }

        public UserRowBuilder WithIsDisabled(bool isDisabled)
        {
            _isDisabled = isDisabled;
            return this;
        }

        public UserRowBuilder WithUserRole(UserRoleRowBuilder userRoleBuilder)
        {
            _userRoleBuilders.Add(userRoleBuilder);
            return this;
        }

        public UserRowBuilder WithUserRefreshToken(UserRefreshTokenRowBuilder userRefreshTokenRowBuilder)
        {
            _userRefreshTokenRowBuilders.Add(userRefreshTokenRowBuilder);
            return this;
        }

        public UserRow Build()
        {
            return new UserRow
            {
                UserId = _userId,
                Identifier = _identifier,
                PlexAccountId = _plexAccountId,
                Username = _userName,
                Email = _email,
                IsAdmin = _isAdmin,
                IsDisabled = _isDisabled,
                LastLoginUtc = _lastLoginUtc,
                InvalidateTokensBeforeUtc = _invalidateTokensBeforeUtc,
                UserRoles = _userRoleBuilders.Select(x => x.Build()).ToList(),
                UserRefreshTokens = _userRefreshTokenRowBuilders.Select(x => x.Build()).ToList()
            };
        }
    }
}
