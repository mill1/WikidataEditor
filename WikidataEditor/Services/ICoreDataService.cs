using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public interface ICoreDataService
    {
        IWikidataItemDto GetCoreData(string id);
    }
}