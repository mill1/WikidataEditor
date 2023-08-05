using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public interface IWikidataRestService
    {
        IWikidataItemDto GetDataOnHuman(string id);
    }
}