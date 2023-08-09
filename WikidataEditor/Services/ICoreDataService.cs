using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public interface ICoreDataService
    {
        IWikidataItemDto Get(string id);
    }
}