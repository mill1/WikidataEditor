using WikidataEditor.Common;
using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public class StatementService
    {
        private readonly IWikidataHelper _wikidataHelper;

        public StatementService(IHttpClientWikidataApi httpClientWikidataApi, IWikidataHelper wikidataHelper)
        {
            _wikidataHelper = wikidataHelper;
        }

        public async Task<IEnumerable<StatementsDto>> Get(string id)
        {
            return await _wikidataHelper.GetStatements(id);
        }

        public async Task<IEnumerable<StatementsDto>> Get(string id, string property)
        {
            return await _wikidataHelper.GetStatement(id, property);
        }
    }
}
