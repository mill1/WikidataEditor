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

        public async Task <IEnumerable<EntityTextDto>> Get(string id)
        {
            string uri = "items/" + id + "/descriptions";
            var jsonString = await _httpClientWikidataApi.GetStringAsync(uri);
            JObject jsonObject = JObject.Parse(jsonString);
            var codes = jsonObject.ToObject<LanguageCodes>();

            var descriptions = codes.GetType().GetProperties()
            .Where(c => c.PropertyType == typeof(string))
            .Select(c =>
                {
                    return new EntityTextDto
                    {
                        LanguageCode = c.Name,
                        Value = (string)c.GetValue(codes)
                    };
                })
            .Where(et => !string.IsNullOrEmpty(et.Value));

            return descriptions;
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
