using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess.Repositories
{
    public interface IUserRepository : IBaseRepository
    {
        Task<List<UserRow>> GetAllUsers(bool includeDisabled, bool includeAdmin);
        Task<UserRow> GetUser(int userId);
        Task<UserRow> GetUser(string email);
        Task<UserRow> GetUser(Guid identifier);
        Task<UserRow> GetUserFromPlexId(int plexAccountId);
        void Add(UserRow userRow);
        Task<UserRow> GetAdmin();
    }
}
