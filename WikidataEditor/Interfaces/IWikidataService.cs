using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IWikidataService
    {
        WikidataItemHumanDto GetDataOnHuman(string id);
    }
}