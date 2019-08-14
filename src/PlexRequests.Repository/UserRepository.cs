using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(string connectionString, string databaseName) :
            base(connectionString, databaseName, "Users")
        {
        }

        public async Task<List<User>> GetAllUsers(bool includeDisabled = false, bool includeAdmin = false)
        {
            var query = Collection.AsQueryable();
            
            if (!includeDisabled)
            {
                query = query.Where(x => !x.IsDisabled);
            }

            if (!includeAdmin)
            {
                query = query.Where(x => !x.IsAdmin);
            }
            
            return await query.ToListAsync();
        }

        public async Task<User> GetAdmin()
        {
            var users = await Collection.FindAsync(x => x.IsAdmin && !x.IsDisabled);
            return await users.FirstOrDefaultAsync();
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
