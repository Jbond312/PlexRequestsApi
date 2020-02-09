﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {

        public UserRepository(PlexRequestsDataContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserRow>> GetAllUsers(bool includeDisabled, bool includeAdmin)
        {
            return await GetUsers().ToListAsync();
        }

        public async Task<UserRow> GetUser(int userId)
        {
            return await GetUsers().FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserRow> GetUser(string email)
        {
            return await GetUsers().FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<UserRow> GetUser(Guid identifier)
        {
            return await GetUsers().FirstOrDefaultAsync(x => x.Identifier == identifier);
        }

        public async Task<UserRow> GetUserFromPlexId(int plexAccountId)
        {
            return await GetUsers().FirstOrDefaultAsync(x => x.PlexAccountId == plexAccountId);
        }

        public void Add(UserRow userRow)
        {
            DbContext.Users.Add(userRow);
        }

        public async Task<UserRow> GetAdmin()
        {
            return await GetUsers().FirstOrDefaultAsync(x => x.IsAdmin);
        }

        private IQueryable<UserRow> GetUsers()
        {
            return DbContext.Users.Include(x => x.UserRoles).Include(x => x.UserRefreshTokens);
        }
    }
}