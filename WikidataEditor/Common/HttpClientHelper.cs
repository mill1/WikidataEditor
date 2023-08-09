using Newtonsoft.Json;
using System.Text;
using WikidataEditor.Models;

namespace WikidataEditor.Common
{
    public class HttpClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientHelper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task PutAsync(string uri, object request)
        {
            string json = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientWikidataRestApi);

            var response =  await httpClient.PutAsync(uri, requestContent);
            response.EnsureSuccessStatusCode();
        }
    }
}
