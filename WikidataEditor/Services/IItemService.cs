using WikidataEditor.Dtos;
using WikidataEditor.Dtos.CoreData;

namespace WikidataEditor.Services
{
    public interface IItemService
    {
        FlatWikidataItemDto GetCoreData(string id);
        WikidataItemDto Get(string id);
    }
}