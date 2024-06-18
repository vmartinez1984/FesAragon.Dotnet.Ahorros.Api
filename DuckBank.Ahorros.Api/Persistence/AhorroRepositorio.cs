using DuckBank.Ahorros.Api.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DuckBank.Ahorros.Api.Persistence
{
    public class AhorroRepositorio
    {
        private readonly IMongoCollection<Ahorro> _collection;

        public AhorroRepositorio(IConfiguration configurations)
        {
            var mongoClient = new MongoClient(configurations.GetConnectionString("mongoDb"));
            var mongoDatabase = mongoClient.GetDatabase(configurations.GetConnectionString("mongoDbNombre"));
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

        internal async Task<List<Ahorro>> ObtenerAsync()
        {
            List<Ahorro> ahorros;
            // FilterDefinition<Ahorro> filter;

            // filter = Builders<Ahorro>.Filter.Eq("ClienteId", clienteId);
            ahorros = (await _collection.FindAsync(_ => true)).ToList();

            return ahorros;
        }

        internal async Task<List<Ahorro>> ObtenerListaDeAhorrosPorClienteIdAsync(string clienteId)
        {
            List<Ahorro> ahorros;
            FilterDefinition<Ahorro> filter;

            filter = Builders<Ahorro>.Filter.Eq("ClienteId", clienteId);
            ahorros = (await _collection.FindAsync(filter)).ToList();

            return ahorros;
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
            catch (Exception)
            {
                return null;
            }
        }

        internal async Task<List<Ahorro>> GetAsync(PagerEntity pager)
        {
            List<Ahorro> entities;
            FilterDefinition<Ahorro> filter;

            if (string.IsNullOrEmpty(pager.Search))
                filter = Builders<Ahorro>.Filter.Where(_ => true);
            else
                filter = Builders<Ahorro>.Filter
                .Where(x => x.Nombre.ToLower().Contains(pager.Search.ToLower()));

            entities = await _collection.Find(filter)
                .Sort("{Id:1}")
                .Skip((pager.PageCurrent - 1) * pager.RecordsPerPage)
                .Limit(pager.RecordsPerPage)
                .ToListAsync();

            pager.TotalRecordsFiltered = entities.Count();
            pager.TotalRecords = (int)await _collection.CountDocumentsAsync(new BsonDocument());

            return entities;
        }
    }

    public class PagerEntity
    {
        public int PageCurrent { get; set; } = 1;

        public int RecordsPerPage { get; set; } = 10;

        public string Search { get; set; }

        public string SortColumn { get; set; }

        public string SortColumnDir { get; set; }

        public int TotalRecords { get; set; }

        public int TotalRecordsFiltered { get; set; }
    }
}