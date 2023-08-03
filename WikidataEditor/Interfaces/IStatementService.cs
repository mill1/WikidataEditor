using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IStatementService
    {
        HumanDto GetWikidataStatements(string id);
    }
}