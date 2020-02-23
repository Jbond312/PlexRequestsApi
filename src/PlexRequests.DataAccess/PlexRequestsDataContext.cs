using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess
{
    public class PlexRequestsDataContext : DbContext
    {
        public PlexRequestsDataContext(DbContextOptions<PlexRequestsDataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var createdOrModifiedEntities = ChangeTracker.Entries()
                .Where(x => x.Entity is TimestampRow
                            && (x.State == EntityState.Modified || x.State == EntityState.Added));

            foreach(var entityEntry in createdOrModifiedEntities)
            {
                var timeStampEntity = ((TimestampRow)entityEntry.Entity);
                timeStampEntity.UpdatedUtc = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    timeStampEntity.CreatedUtc = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<UserRow> Users { get; set; }
        public DbSet<UserRoleRow> UserRoles { get; set; }
        public DbSet<UserRefreshTokenRow> UserRefreshTokens { get; set; }
        public DbSet<PlexServerRow> PlexServers { get; set; }
        public DbSet<PlexLibraryRow> PlexLibraries { get; set; }
        public DbSet<PlexMediaItemRow> PlexMediaItems { get; set; }
        public DbSet<PlexSeasonRow> PlexSeasons { get; set; }
        public DbSet<PlexEpisodeRow> PlexEpisodes { get; set; }
        public DbSet<IssueRow> Issues { get; set; }
        public DbSet<IssueCommentRow> IssueComments { get; set; }
        public DbSet<MovieRequestRow> MovieRequests { get; set; }
        public DbSet<MovieRequestAgentRow> MovieRequestAgents { get; set; }
        public DbSet<TvRequestRow> TvRequests { get; set; }
        public DbSet<TvRequestAgentRow> TvRequestAgents { get; set; }
        public DbSet<TvRequestUserRow> TvRequestUsers { get; set; }
        public DbSet<TvRequestSeasonRow> TvRequestSeasons { get; set; }
        public DbSet<TvRequestEpisodeRow> TvRequestEpisodes { get; set; }
    }
}
