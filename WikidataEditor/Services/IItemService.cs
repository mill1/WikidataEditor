using Newtonsoft.Json.Linq;
using WikidataEditor.Dtos.CoreData;

namespace WikidataEditor.Services
{
    public interface IItemService
    {
        FlatWikidataItemDto GetCoreData(string id);
        JObject Get(string id);
    }
}