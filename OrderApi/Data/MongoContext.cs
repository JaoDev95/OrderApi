using MongoDB.Driver;

namespace OrderApi.Data;

public class MongoContext
{
    public IMongoDatabase Database { get; }

    public MongoContext(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("MongoDb");
        var databaseName = config["MongoDatabaseName"];

        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }
}