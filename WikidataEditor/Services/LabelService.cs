using Newtonsoft.Json;
using System.Text;
using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Services
{
    // TODO: interface + tests
    // TODO: LabelService https://www.wikidata.org/wiki/Q114282849
    public class LabelService
    {
        private readonly HttpClientHelper _httpClientHelper;

        public LabelService(HttpClientHelper httpClientHelper)
        {
            _httpClientHelper = httpClientHelper;
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
            await _httpClientHelper.PutAsync(uri, request);            
        }
    }
}
