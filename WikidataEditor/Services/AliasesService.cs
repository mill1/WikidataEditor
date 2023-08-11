using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Services
{
    public class AliasesService
    {
        private readonly IWikidataHelper _wikidataHelper;

        public AliasesService(IWikidataHelper wikidataHelper)
        {
            _wikidataHelper = wikidataHelper;
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id)
        {
            return await _wikidataHelper.GetAliases(id);
        }

        public async Task<IEnumerable<EntityTextDto>> Get(string id, string languageCode)
        {
            return await _wikidataHelper.GetEntityText(id, languageCode, "aliases");
        }
    }
}
