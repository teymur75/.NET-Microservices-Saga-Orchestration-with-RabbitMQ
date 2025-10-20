using MongoDB.Driver;

namespace Stock.API.Services
{
    public class MongoDBService 
    {
        readonly IMongoDatabase _db;
        public MongoDBService(IConfiguration cfg)
        {
            MongoClient client = new(cfg.GetConnectionString("MongoDB"));
            _db = client.GetDatabase("StockDB");
        }

        public IMongoCollection<T> GetCollection<T>() => _db.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}
