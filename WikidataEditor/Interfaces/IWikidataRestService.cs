using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IWikidataRestService
    {
        HumanDto GetDataOnHuman(string id);
    }
}