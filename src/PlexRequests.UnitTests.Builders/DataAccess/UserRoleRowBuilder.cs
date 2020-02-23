using PlexRequests.Core.Auth;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class UserRoleRowBuilder : IBuilder<UserRoleRow>
    {
        private int _userRoleId;
        private string _role;

        public UserRoleRowBuilder()
        {
            _userRoleId = 1;
            _role = PlexRequestRoles.User;
        }

        public UserRoleRowBuilder WithId(int id)
        {
            _userRoleId = id;
            return this;
        }

        public UserRoleRowBuilder WithRole(string role)
        {
            _role = role;
            return this;
        }

        public UserRoleRow Build()
        {
            return new UserRoleRow
            {
                UserRoleId = _userRoleId,
                Role = _role,
            };
        }
    }
}
