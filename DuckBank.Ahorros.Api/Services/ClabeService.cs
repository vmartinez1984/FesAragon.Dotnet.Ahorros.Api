using Newtonsoft.Json.Linq;

namespace DuckBank.Ahorros.Api.Services
{
    public class ClabeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ClabeService> _logger;

        public ClabeService(IHttpClientFactory httpClientFactory, ILogger<ClabeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string ObtenerClabe(string ahorroId)
        {
            try
            {
                HttpClient httpClient;
                HttpResponseMessage response;
                HttpRequestMessage request;

                httpClient = _httpClientFactory.CreateClient();
                request = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.1:3021/api/clabe?ahorroId=" + ahorroId);
                response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject;

                    jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    return jsonObject["clabe"].ToString();
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(), ex, "Excepcion en la clabe");
                return string.Empty;
            }
        }
    }
}
