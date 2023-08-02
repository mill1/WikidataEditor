using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IStatementService
    {
        WikidataStatementsDto GetWikidataStatements(string id);
    }
}