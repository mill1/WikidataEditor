using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Services
{
    public class DescriptionService
    {
        private readonly HttpClientWikidataApi _httpClientWikidataApi;

        public DescriptionService(HttpClientWikidataApi httpClientWikidataApi)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
        }

        //public IEnumerable

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
