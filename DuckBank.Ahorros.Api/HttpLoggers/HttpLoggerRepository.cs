using MongoDB.Driver;

namespace DuckBank.Ahorros.Api.HttpLoggers
{
    public class HttpLoggerRepository
    {
        private readonly IMongoCollection<HttpLoggerEntity> _collection;
        public HttpLoggerRepository(IConfiguration configurations)
        {
            var mongoClient = new MongoClient(
                configurations.GetConnectionString("mongoDbLogs")
            );
            //var nombreDeLaDb = configurations.GetSection("DuckBankAhorros").Value;
            var mongoDatabase = mongoClient.GetDatabase("DuckBank_Logs");
            _collection = mongoDatabase.GetCollection<HttpLoggerEntity>("Http");
        }

        public async Task<string> AgregarAsync(HttpLoggerEntity entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);

                return entity._id;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
