using System;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Store;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUser(Guid id)
        {
            return await _userRepository.GetUser(id);
        }

        public async Task<User> GetUserFromPlexId(int plexAccountId)
        {
            return await _userRepository.GetUser(plexAccountId);
        }

        public async Task<User> CreateUser(User user)
        {
            return await _userRepository.CreateUser(user);
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
            var users = await _userRepository.GetAllUsers();

            return users.Any(x => x.IsAdmin);
        }
    }
}
