using Newtonsoft.Json;
using System.Text;
using WikidataEditor.Common;
using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public class DescriptionService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DescriptionService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task UpdateDescription(string id, string description, string languageCode)
        {
            var request = new UpdateDescriptionRequestDto
            {
                description = description,
                tags = new string[0],
                bot = false,
                comment = "Updated description"
            };

            string json = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientWikidataRestApi);
            var uri = $"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/{id}/descriptions/{languageCode}";
            var response = await httpClient.PutAsync(uri, requestContent);
            response.EnsureSuccessStatusCode();
        }
    }
}
