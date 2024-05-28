using Newtonsoft.Json.Linq;

namespace DuckBank.Ahorros.Api.Services
{
    public class TarjetaDeDebitoService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TarjetaDeDebitoService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string ObtenerTarjeta()
        {
            HttpClient httpClient;
            HttpResponseMessage response;
            HttpRequestMessage request;

            httpClient = _httpClientFactory.CreateClient();
            request = new HttpRequestMessage(HttpMethod.Get, "https://utilidades.vmartinez84.xyz/api/TarjetasDeDebito");
            response = httpClient.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                JObject jsonObject;

                jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                return jsonObject["numeroDeTarjeta"].ToString();
            }
            else
                return string.Empty;
        }
    }
}
