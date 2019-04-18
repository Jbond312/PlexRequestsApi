using MongoDB.Bson;
using MongoDB.Driver;

namespace PlexRequests.Repository
{
    public class BaseRepository<T>
    {
        protected IMongoCollection<T> Collection { get; }

        public BaseRepository(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName, new MongoDatabaseSettings
            {
                GuidRepresentation = GuidRepresentation.Standard
            });
            Collection = database.GetCollection<T>(collectionName);
        }
    }
}
