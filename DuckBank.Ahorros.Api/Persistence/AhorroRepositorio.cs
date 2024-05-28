using DuckBank.Ahorros.Api.Entities;
using MongoDB.Driver;

namespace DuckBank.Ahorros.Api.Persistence
{
    public class AhorroRepositorio
    {

        private readonly IMongoCollection<Ahorro> _collection;
        public AhorroRepositorio(IConfiguration configurations)
        {
            var mongoClient = new MongoClient(
                configurations.GetConnectionString("mongoDb")
            );
            var mongoDatabase = mongoClient.GetDatabase("DuckBank");
            _collection = mongoDatabase.GetCollection<Ahorro>("Ahorros");
        }

        internal async Task ActualizarAsync(Ahorro ahorro)
        {            
            await _collection.ReplaceOneAsync(a => a._id == ahorro._id, ahorro);
        }

        internal async Task<int> AgregarAsync(Ahorro item)
        {
            item.Id = ((int)await _collection.CountDocumentsAsync(_ => true)) + 1;
            await _collection.InsertOneAsync(item);

            return item.Id;
        }

        internal async Task<Ahorro> ObtenerPorIdAsync(string id)
        {
            try
            {

                Ahorro ahorro;
                FilterDefinition<Ahorro> filter;
                if (id.Length == 32)
                    filter = Builders<Ahorro>.Filter.Eq("Guid", id);
                else
                    filter = Builders<Ahorro>.Filter.Eq("Id", id);
                ahorro = (await _collection.FindAsync(filter)).FirstOrDefault();

                return ahorro;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}