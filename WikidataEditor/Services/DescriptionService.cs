using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Services
{
    public class DescriptionService
    {
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;
        private readonly IWikidataHelper _wikidataHelper;

        public DescriptionService(IHttpClientWikidataApi httpClientWikidataApi, IWikidataHelper wikidataHelper)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
            _wikidataHelper = wikidataHelper;
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id)
        {            
            return await _wikidataHelper.GetEntityTexts(id, "descriptions");
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id, string languageCode)
        {
            return await _wikidataHelper.GetEntityText(id, languageCode, "descriptions");
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
