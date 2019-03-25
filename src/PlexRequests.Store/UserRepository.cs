using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(string connectionString, string databaseName) :
            base(connectionString, databaseName, "Users")
        {
        }

        public async Task<List<User>> GetAllUsers()
        {
            var users = await Collection.FindAsync(x => !x.IsDisabled);
            return await users.ToListAsync();
        }

        public async Task<User> GetUser(Guid id)
        {
            var findCursor = await Collection.FindAsync(x => x.Id == id && !x.IsDisabled);
            return await findCursor.FirstOrDefaultAsync();
        }

        public async Task<User> GetUser(int plexAccountId)
        {
            var findCursor = await Collection.FindAsync(x => x.PlexAccountId == plexAccountId && !x.IsDisabled);
            return await findCursor.FirstOrDefaultAsync();
        }

        public async Task<User> GetUser(string email)
        {
            var findCursor = await Collection.FindAsync(x => x.Email == email && !x.IsDisabled);
            return await findCursor.FirstOrDefaultAsync();
        }

        public async Task<User> CreateUser(User user)
        {
            await Collection.InsertOneAsync(user);

            return user;
        }

        public async Task<User> UpdateUser(User user)
        {
            var filter = new FilterDefinitionBuilder<User>().Eq(x => x.Id, user.Id);
            var updatedUser = await Collection.FindOneAndReplaceAsync(filter, user);
            return updatedUser;
        }

        public async Task DeleteUser(Guid userId)
        {
            var filter = new FilterDefinitionBuilder<User>().Eq(x => x.Id, userId);
            var update = new UpdateDefinitionBuilder<User>().Set(x => x.IsDisabled, true);
            await Collection.FindOneAndUpdateAsync(filter, update);
        }
    }
}
