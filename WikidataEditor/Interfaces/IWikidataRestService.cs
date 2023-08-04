using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IWikidataRestService
    {
        WikidataItemHumanDto GetDataOnHuman(string id);
    }
}