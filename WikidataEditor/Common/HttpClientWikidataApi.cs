using Newtonsoft.Json;
using System.Text;

namespace WikidataEditor.Common
{
    public class HttpClientWikidataApi : IHttpClientWikidataApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        public HttpClientWikidataApi(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient(Constants.HttpClientWikidataRestApi);
        }

        public async Task<string> GetStringAsync(string uri)
        {
            return await _httpClient.GetStringAsync(uri);
        }

        public async Task PutAsync(string uri, object request)
        {
            var response = await _httpClient.PutAsync(uri, GetRequestContent(request));
            response.EnsureSuccessStatusCode();
        }

        public async Task PostAsync(string uri, object request)
        {            
            var response = await _httpClient.PostAsync(uri, GetRequestContent(request));
            response.EnsureSuccessStatusCode();
        }

        private static StringContent GetRequestContent(object request)
        {
            string json = JsonConvert.SerializeObject(request);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
