using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Services
{
    public class LabelService
    {
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;

        public LabelService(IHttpClientWikidataApi httpClientWikidataApi)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
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
