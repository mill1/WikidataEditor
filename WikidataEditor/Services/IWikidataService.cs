using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public interface IWikidataService
    {
        IWikidataItemDto GetCoreData(string id);
    }
}