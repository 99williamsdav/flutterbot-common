using Common.ModelInterfaces;
using Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Common.Persistence
{
    public class MongoConfigGeneric : IGetConfig
    {
        private readonly string _connectionString;

        private readonly string _collectionName;

        private readonly string _dbName;

        public MongoConfigGeneric(string dbName, string connectionString, string collectionName)
        {
            this._dbName = dbName;
            this._connectionString = connectionString;
            this._collectionName = collectionName;
        }

        public T GetConfig<T>(string id = null) where T : IIdentifier
        {
            MongoInit.Initialize<T>();
            var client = new MongoClient(this._connectionString);
            //var server = client.GetServer();
            var database = client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);

            var qry = Builders<T>.Filter.Eq("_id", id ?? BsonValue.Create(typeof(T).Name));
            return col.Find(qry).Single();
        }
    }
}
