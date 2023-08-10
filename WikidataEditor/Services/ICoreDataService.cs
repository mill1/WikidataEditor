using WikidataEditor.Dtos.CoreData;

namespace WikidataEditor.Services
{
    public interface ICoreDataService
    {
        IWikidataItemDto Get(string id);
    }
}