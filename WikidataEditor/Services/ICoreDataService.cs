using WikidataEditor.Dtos.CoreData;

namespace WikidataEditor.Services
{
    public interface ICoreDataService
    {
        FlatWikidataItemDto Get(string id);
    }
}