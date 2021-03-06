﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess.Repositories
{
    public interface IUserRepository : IBaseRepository
    {
        Task<List<UserRow>> GetAllUsers(bool includeDisabled, bool includeAdmin);
        Task<UserRow> GetUser(int userId);
        Task<UserRow> GetUser(string email);
        Task<UserRow> GetUser(Guid identifier);
        Task<UserRow> GetUserFromPlexId(int plexAccountId);
        Task Add(UserRow user);
        Task<UserRow> GetAdmin();
        void DeleteExpiredUserTokens();
    }

    public class UserRepository : BaseRepository, IUserRepository
    {

        public UserRepository(PlexRequestsDataContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserRow>> GetAllUsers(bool includeDisabled, bool includeAdmin)
        {
            var users = GetUsers();

            if (!includeDisabled)
            {
                users = users.Where(x => !x.IsDisabled);
            }

            if (!includeAdmin)
            {
                users = users.Where(x => !x.IsAdmin);
            }

            return await users.ToListAsync();
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

        public async Task Add(UserRow user)
        {
            await base.Add(user);
        }

        public async Task<UserRow> GetAdmin()
        {
            return await GetUsers().FirstOrDefaultAsync(x => x.IsAdmin);
        }

        /// <summary>
        /// Deletes any expired refresh tokens that expired at least 1 day ago
        /// </summary>
        public void DeleteExpiredUserTokens()
        {
            var tokensToDelete = DbContext.UserRefreshTokens.Where(x => x.ExpiresUtc.AddDays(1) < DateTime.UtcNow);

            DbContext.UserRefreshTokens.RemoveRange(tokensToDelete);
        }

        private IQueryable<UserRow> GetUsers()
        {
            return DbContext.Users.Include(x => x.UserRoles).Include(x => x.UserRefreshTokens);
        }
    }
}