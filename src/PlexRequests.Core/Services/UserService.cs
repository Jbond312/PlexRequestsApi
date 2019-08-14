using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<User>> GetAllUsers(bool includeDisabled = false, bool includeAdmin = false)
        {
            return await _userRepository.GetAllUsers(includeDisabled, includeAdmin);
        }

        public async Task<User> GetUser(Guid id)
        {
            return await _userRepository.GetUser(id);
        }

        public async Task<User> GetUserFromPlexId(int plexAccountId)
        {
            return await _userRepository.GetUser(plexAccountId);
        }

        public async Task CreateUser(User user)
        {
            await _userRepository.CreateUser(user);
        }

        public async Task<User> UpdateUser(User user)
        {
            return await _userRepository.UpdateUser(user);
        }

        public async Task<bool> UserExists(string email)
        {
            var user = await _userRepository.GetUser(email);
            return user != null;
        }

        public async Task<bool> IsAdminCreated()
        {
            var adminUser = await _userRepository.GetAdmin();

            return adminUser != null;
        }
    }
}
