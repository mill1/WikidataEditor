using Newtonsoft.Json;
using System.Text;

namespace WikidataEditor.Common
{
    public class HttpClientEnglishWikipediaApi : IHttpClientEnglishWikipediaApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        public HttpClientEnglishWikipediaApi(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient(Constants.HttpClientEnglishWikipediaApi);
        }

        public async Task<string> GetStringAsync(string uri)
        {
            return await _httpClient.GetStringAsync(uri);
        }
    }
}
