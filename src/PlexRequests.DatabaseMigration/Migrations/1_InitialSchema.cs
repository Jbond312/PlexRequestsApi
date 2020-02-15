using FluentMigrator;

namespace PlexRequests.DatabaseMigration.Migrations
{
    [Migration(1)]
    public class InitialSchema : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("1_InitialSchema.sql");
        }

        public override void Down()
        {
            Execute.EmbeddedScript("1_InitialSchema_Rollback.sql");
        }
    }
}
