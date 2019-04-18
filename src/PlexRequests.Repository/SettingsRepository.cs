using System.Threading.Tasks;
using MongoDB.Driver;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public class SettingsRepository : BaseRepository<Settings>, ISettingsRepository
    {
        public SettingsRepository(string connectionString, string databaseName) : 
            base(connectionString, databaseName, "Settings")
        {
        }

        public async Task<Settings> GetSettings()
        {
            return await GetExistingSettings();
        }

        public async Task UpdateSettings(Settings settings)
        {
            var firstSetting = await GetExistingSettings();

            if (firstSetting == null)
            {
                await Collection.InsertOneAsync(settings);
            }
            else
            {
                settings.Id = firstSetting.Id;
                await Collection.ReplaceOneAsync(x => x.Id == firstSetting.Id, settings);
            }
        }

        public async Task PrimeSettings(Settings settings, bool overwrite)
        {
            var firstSetting = await GetExistingSettings();

            if (firstSetting == null)
            {
                await Collection.InsertOneAsync(settings);
            }
            else if (overwrite)
            {
                settings.Id = firstSetting.Id;
                await Collection.ReplaceOneAsync(x => x.Id == firstSetting.Id, settings);
            }
        }

        private async Task<Settings> GetExistingSettings()
        {
            var findCursor = await Collection.FindAsync(FilterDefinition<Settings>.Empty);
            return await findCursor.FirstOrDefaultAsync();
        }
    }
}