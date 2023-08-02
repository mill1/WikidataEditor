using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;

namespace WikidataEditor.Services
{
    public class StatementService : IStatementService
    {
        private readonly IWikidataService _wikidataService;

        public StatementService(IWikidataService wikidataService)
        {
            _wikidataService = wikidataService;
        }

        public WikidataStatementsDto GetWikidataStatements(string id)
        {
            return _wikidataService.GetStatements(id);
        }
    }
}
