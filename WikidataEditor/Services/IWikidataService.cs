using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public interface IWikidataService
    {
        IWikidataItemDto GetDataOnHuman(string id);
    }
}