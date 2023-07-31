using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public class StatementService
    {
        private readonly WikidataService _wikidataService;

        public StatementService(WikidataService wikidataService) 
        {
            _wikidataService = wikidataService;
        }

        public WikidataStatementsDto GetWikidataStatements(string id)
        {
            return _wikidataService.GetStatements(id);
        }
    }
}
