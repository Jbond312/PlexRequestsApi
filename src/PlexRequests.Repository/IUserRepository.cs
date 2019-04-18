using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsers();
        Task<User> GetAdmin();
        Task<User> GetUser(Guid id);
        Task<User> GetUser(int plexAccountId);
        Task<User> GetUser(string userName);
        Task<User> CreateUser(User user);
        Task<User> UpdateUser(User user);
        Task DeleteUser(Guid userId);
    }
}
