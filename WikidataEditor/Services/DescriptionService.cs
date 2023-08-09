using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public class DescriptionService
    {
        private readonly HttpClient _client;

        public DescriptionService(HttpClient httpClient) 
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "C# Application");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task UpdateDescription(string id, string description)
        {
            var request = new UpdateDescriptionRequestDto
            {
                description = description,
                tags = new string[0],
                bot = false,
                comment = "Updated description for Dutch language"
            };

            string json = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = $"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/{id}/descriptions/nl";
            var response = await _client.PutAsync(uri, requestContent);
            response.EnsureSuccessStatusCode();

        }
    }
}
