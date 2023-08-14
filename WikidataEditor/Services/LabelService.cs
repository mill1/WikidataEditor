using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Services
{
    public class LabelService
    {
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;
        private readonly IWikidataHelper _wikidataHelper;

        public LabelService(IHttpClientWikidataApi httpClientWikidataApi, IWikidataHelper wikidataHelper)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
            _wikidataHelper = wikidataHelper;
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id)
        {
            return await _wikidataHelper.GetEntityTexts(id, "labels");
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id, string languageCode)
        {
            return await _wikidataHelper.GetEntityText(id, languageCode, "labels");
        }

        public async Task UpsertLabel(string id, string label, string languageCode, string comment)
        {
            var request = new UpdateLabelRequestDto
            {
                label = label,
                tags = new string[0],
                bot = false,
                comment = comment
            };

            string uri = $"items/{id}/labels/{languageCode}";
            await _httpClientWikidataApi.PutAsync(uri, request);
        }
    }
}
