using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly IMongoCollection<Settings> _collection;

        public SettingsRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName, new MongoDatabaseSettings
            {
                GuidRepresentation = GuidRepresentation.Standard
            });
            _collection = database.GetCollection<Settings>("Settings");
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
                await _collection.InsertOneAsync(settings);
            }
            else
            {
                settings.Id = firstSetting.Id;
                await _collection.ReplaceOneAsync(x => x.Id == firstSetting.Id, settings);
            }
        }

        public async Task PrimeSettings(Settings settings, bool overwrite)
        {
            var firstSetting = await GetExistingSettings();

            if (firstSetting == null)
            {
                await _collection.InsertOneAsync(settings);
            }
            else if (overwrite)
            {
                settings.Id = firstSetting.Id;
                await _collection.ReplaceOneAsync(x => x.Id == firstSetting.Id, settings);
            }
        }

        private async Task<Settings> GetExistingSettings()
        {
            var findCursor = await _collection.FindAsync(FilterDefinition<Settings>.Empty);
            return await findCursor.FirstOrDefaultAsync();
        }
    }
}