using Newtonsoft.Json.Linq;

namespace DuckBank.Ahorros.Api.Services
{
    public class ClabeService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ClabeService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string ObtenerClabe(string ahorroId)
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
    }
}
