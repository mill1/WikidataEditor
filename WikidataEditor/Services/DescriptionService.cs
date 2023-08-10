using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class DescriptionService
    {
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;

        public DescriptionService(IHttpClientWikidataApi httpClientWikidataApi)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id)
        {
            string uri = "items/" + id + "/descriptions";
            var jsonString = await _httpClientWikidataApi.GetStringAsync(uri);
            JObject jsonObject = JObject.Parse(jsonString);

            var descriptions = new List<EntityTextDto>();

            foreach (var lc in jsonObject.ToObject<dynamic>())
            {
                descriptions.Add(new EntityTextDto
                {
                    LanguageCode = ((JProperty)lc).Name,
                    Value = (string)((JProperty)lc).Value
                });
            }
            return descriptions;
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id, string languageCode)
        {
            string uri = "items/" + id + "/descriptions/" + languageCode;
            var result = await _httpClientWikidataApi.GetStringAsync(uri);

            return new List<EntityTextDto>
            {
                new EntityTextDto
                {
                    LanguageCode = languageCode,
                    Value = JsonConvert.DeserializeObject<string>(result)
                }
            };
        }

        public async Task UpsertDescription(string id, string description, string languageCode, string comment)
        {
            var request = new UpdateDescriptionRequestDto
            {
                description = description,
                tags = new string[0],
                bot = false,
                comment = comment
            };

            string uri = $"items/{id}/descriptions/{languageCode}";
            await _httpClientWikidataApi.PutAsync(uri, request);            
        }
    }
}
