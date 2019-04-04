using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<User> GetUser(Guid id);
        Task<User> GetUserFromPlexId(int plexAccountId);
        Task CreateUser(User user);
        Task<User> UpdateUser(User user);
        Task<bool> UserExists(string email);
        Task<bool> IsAdminCreated();
    }
}
