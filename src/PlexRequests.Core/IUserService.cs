using System;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public interface IUserService
    {
        Task<User> GetUser(Guid id);
        Task<User> GetUserFromPlexId(int plexAccountId);
        Task<User> CreateUser(User user);
        Task<User> UpdateUser(User user);
        Task<bool> UserExists(string email);
        Task<bool> IsAdminCreated();
    }
}
