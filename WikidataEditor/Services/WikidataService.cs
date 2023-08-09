using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public class WikidataService : IWikidataService
    {
        private readonly ICoreDataService _wikidataService;

        public WikidataService(ICoreDataService wikidataService)
        {
            _wikidataService = wikidataService;
        }

        public IWikidataItemDto GetCoreData(string id)
        {
            return _wikidataService.GetCoreData(id);
        }
    }
}
