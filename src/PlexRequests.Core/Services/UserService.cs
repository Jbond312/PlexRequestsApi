using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Repositories;

namespace PlexRequests.Core.Services
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserRow>> GetAllUsers(bool includeDisabled = false, bool includeAdmin = false)
        {

            return await _userRepository.GetAllUsers(includeDisabled, includeAdmin);
        }

        public async Task<UserRow> GetUser(int userId)
        {
            return await _userRepository.GetUser(userId);
        }

        public async Task<UserRow> GetUser(Guid identifier)
        {
            return await _userRepository.GetUser(identifier);
        }

        public async Task<UserRow> GetUserFromPlexId(int plexAccountId)
        {
            return await _userRepository.GetUserFromPlexId(plexAccountId);
        }
        
        public void AddUser(UserRow userRow)
        {
            _userRepository.Add(userRow);
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
