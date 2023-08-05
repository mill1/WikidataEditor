using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public class WikidataService : IWikidataService
    {
        private readonly IWikidataRestService _wikidataService;

        public WikidataService(IWikidataRestService wikidataService)
        {
            _wikidataService = wikidataService;
        }

        public IWikidataItemDto GetDataOnHuman(string id)
        {
            return _wikidataService.GetData(id);
        }
    }
}
