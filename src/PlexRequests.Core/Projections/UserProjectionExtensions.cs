using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Models;

namespace PlexRequests.Core.Projections
{
    public static class UserProjectionExtensions
    {
        public static IEnumerable<User> ToModel(this IEnumerable<UserRow> userRows)
        {
            return userRows.
                Select(
                    u => new User(
                        u.UserId,
                        u.Identifier,
                        u.PlexAccountId,
                        u.Username,
                        u.Email,
                        u.IsAdmin,
                        u.IsDisabled,
                        u.LastLoginUtc,
                        u.InvalidateTokensBeforeUtc
                    ));
        }

        public static User ToModel(this UserRow userRow)
        {
            return new User(
                        userRow.UserId,
                        userRow.Identifier,
                        userRow.PlexAccountId,
                        userRow.Username,
                        userRow.Email,
                        userRow.IsAdmin,
                        userRow.IsDisabled,
                        userRow.LastLoginUtc,
                        userRow.InvalidateTokensBeforeUtc
                    );
        }
    }
}
