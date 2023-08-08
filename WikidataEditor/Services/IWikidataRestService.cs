using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public interface IWikidataRestService
    {
        IWikidataItemDto GetCoreData(string id);
    }
}