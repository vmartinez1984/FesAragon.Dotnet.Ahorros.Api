using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Text;
using MongoDB.Driver;

namespace DuckBank.Ahorros.Api.Middlewares
{
    public class RequestResponseMiddleware
    {
        private RequestDelegate _next;        
        private readonly RequestResponseRepository _requestRepository;

        public RequestResponseMiddleware(
            RequestDelegate next,            
            RequestResponseRepository requestRepository
        )
        {
            _next = next;            
            _requestRepository = requestRepository;
        }

        private async Task<RequestResponseEntity> AnalizeRequest(HttpContext context)
        {
            RequestResponseEntity requestDtoIn;

            using (StreamReader stream = new StreamReader(context.Request.Body))
            {
                string path;
                string queryString;
                string header;
                string body;
                string method;


                path = context.Request.Path;
                queryString = context.Request.QueryString.Value;
                header = JsonConvert.SerializeObject(context.Request.Headers).Replace("[", string.Empty).Replace("]", string.Empty);
                method = context.Request.Method;
                body = await stream.ReadToEndAsync();
                requestDtoIn = new RequestResponseEntity
                {                    
                    RequestBody = body,
                    RequestHeader = header,
                    RequestDateRegistration = DateTime.Now,
                    Path = path + queryString,
                    Method = method
                };

                byte[] bytes = Encoding.UTF8.GetBytes(body);
                context.Request.Body = new MemoryStream(bytes);
            }

            return requestDtoIn;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //await AnalizeRequest(context);
                RequestResponseEntity requestDtoIn;

                requestDtoIn = await AnalizeRequest(context);
                // Store the original body stream for restoring the response body back to its original stream
                var originalBodyStream = context.Response.Body;

                // Create new memory stream for reading the response; Response body streams are write-only, therefore memory stream is needed here to read
                await using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                // Call the next middleware
                await _next(context);

                // Set stream pointer position to 0 before reading
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Read the body from the stream
                var responseBodyText = await new StreamReader(memoryStream).ReadToEndAsync();
                //set data response
                AnalizeResponse(context, requestDtoIn, responseBodyText);
                _ = _requestRepository.AgregarAsync(requestDtoIn);
                // Reset the position to 0 after reading
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Do this last, that way you can ensure that the end results end up in the response.
                // (This resulting response may come either from the redirected route or other special routes if you have any redirection/re-execution involved in the middleware.)
                // This is very necessary. ASP.NET doesn't seem to like presenting the contents from the memory stream.
                // Therefore, the original stream provided by the ASP.NET Core engine needs to be swapped back.
                // Then write back from the previous memory stream to this original stream.
                // (The content is written in the memory stream at this point; it's just that the ASP.NET engine refuses to present the contents from the memory stream.)
                context.Response.Body = originalBodyStream;
                await context.Response.Body.WriteAsync(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                await _next(context);
            }
        }

        private static void AnalizeResponse(HttpContext context, RequestResponseEntity requestDtoIn, string responseBodyText)
        {
            requestDtoIn.ResponseBody = responseBodyText;
            requestDtoIn.ResponseHeader = JsonConvert.SerializeObject(context.Response.Headers).Replace("[", string.Empty).Replace("]", string.Empty);
            requestDtoIn.StatusCode = context.Response.StatusCode;
            requestDtoIn.ResponseDateRegistration = DateTime.Now;
        }
    }

    public class RequestResponseRepository
    {
        private readonly IMongoCollection<RequestResponseEntity> _collection;
        public RequestResponseRepository(IConfiguration configurations)
        {
            var mongoClient = new MongoClient(
                configurations.GetConnectionString("mongoDbLogs")
            );
            //var nombreDeLaDb = configurations.GetSection("DuckBankAhorros").Value;
            var mongoDatabase = mongoClient.GetDatabase("DuckBank_Logs");
            _collection = mongoDatabase.GetCollection<RequestResponseEntity>("RequestResponse");
        }

        public async Task<string> AgregarAsync(RequestResponseEntity entity)
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

    public class RequestResponseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string RequestBody { get; internal set; }
        public string RequestHeader { get; internal set; }
        public DateTime RequestDateRegistration { get; internal set; }
        public string Path { get; internal set; }
        public string Method { get; internal set; }
        public string ResponseBody { get; internal set; }
        public string ResponseHeader { get; internal set; }
        public int StatusCode { get; internal set; }
        public DateTime ResponseDateRegistration { get; internal set; }
    }
}
