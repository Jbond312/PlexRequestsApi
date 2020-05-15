using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Core.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserRow>> GetAllUsers(bool includeDisabled = false, bool includeAdmin = false);
        Task<UserRow> GetUser(int userId);
        Task<UserRow> GetUser(Guid identifier);
        Task<UserRow> GetUserFromPlexId(int plexAccountId);
        Task AddUser(UserRow userRow);
        Task<bool> UserExists(string email);
        Task<bool> IsAdminCreated();
        void DeleteExpiredUserTokens();
    }
}
