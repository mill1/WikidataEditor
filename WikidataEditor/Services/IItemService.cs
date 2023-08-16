using Newtonsoft.Json.Linq;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.CoreData;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public interface IItemService
    {
        FlatWikidataItemDto GetCoreData(string id);
        WikidataItemDto Get(string id);
    }
}