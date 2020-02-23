using System.Diagnostics.CodeAnalysis;
using FluentMigrator;
using FluentMigrator.SqlServer;

namespace PlexRequests.DatabaseMigration.Migrations
{
    [Migration(0)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Create_Enum_Tables : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("0_Create_Enum_Tables.sql");

            Insert.IntoTable("RequestStatuses").InSchema(DatabaseConstants.Schema)
                .WithIdentityInsert()
                .Row(new { RequestStatusId = 1, Name = "PendingApproval" })
                .Row(new { RequestStatusId = 2, Name = "Approved" })
                .Row(new { RequestStatusId = 3, Name = "PartialApproval" })
                .Row(new { RequestStatusId = 4, Name = "Completed" })
                .Row(new { RequestStatusId = 5, Name = "PartialCompletion" })
                .Row(new { RequestStatusId = 6, Name = "Rejected" });

            Insert.IntoTable("AgentTypes").InSchema(DatabaseConstants.Schema)
                .WithIdentityInsert()
                .Row(new { AgentTypeId = 1, Name = "TheMovieDb" })
                .Row(new { AgentTypeId = 2, Name = "TheTvDb" })
                .Row(new { AgentTypeId = 3, Name = "Imdb" });

            Insert.IntoTable("MediaTypes").InSchema(DatabaseConstants.Schema)
                .WithIdentityInsert()
                .Row(new { MediaTypeId = 1, Name = "Movie" })
                .Row(new { MediaTypeId = 2, Name = "Show" });

            Insert.IntoTable("IssueStatuses").InSchema(DatabaseConstants.Schema)
                .WithIdentityInsert()
                .Row(new { IssueStatusId = 1, Name = "Pending" })
                .Row(new { IssueStatusId = 2, Name = "InProgress" })
                .Row(new { IssueStatusId = 3, Name = "Resolved" });
        }

        public override void Down()
        {
            Execute.EmbeddedScript("0_Create_Enum_Tables_Rollback.sql");
        }
    }
}
