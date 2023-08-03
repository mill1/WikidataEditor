using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IWikidataService
    {
        HumanDto GetStatements(string id);
    }
}