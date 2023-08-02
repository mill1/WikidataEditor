using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IWikidataService
    {
        WikidataStatementsDto GetStatements(string id);
    }
}