using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public class WikidataService : IWikidataService
    {
        private readonly ICoreDataService _coreDataService;

        public WikidataService(ICoreDataService coreDataService)
        {
            _coreDataService = coreDataService;
        }

        public IWikidataItemDto GetCoreData(string id)
        {
            return _coreDataService.Get(id);
        }
    }
}
