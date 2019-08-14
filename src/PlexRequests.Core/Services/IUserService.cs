using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers(bool includeDisabled = false, bool includeAdmin = false);
        Task<User> GetUser(Guid id);
        Task<User> GetUserFromPlexId(int plexAccountId);
        Task CreateUser(User user);
        Task<User> UpdateUser(User user);
        Task<bool> UserExists(string email);
        Task<bool> IsAdminCreated();
    }
}
