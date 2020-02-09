using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess
{
    public class PlexRequestsDataContext : DbContext
    {
        public PlexRequestsDataContext(DbContextOptions<PlexRequestsDataContext> options) : base(options)
        {
        }

        public DbSet<UserRow> Users { get; set; }
        public DbSet<UserRoleRow> UserRoles { get; set; }
        public DbSet<UserRefreshTokenRow> UserRefreshTokens { get; set; }
    }
}
