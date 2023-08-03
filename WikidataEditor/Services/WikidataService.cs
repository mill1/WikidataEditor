using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;

namespace WikidataEditor.Services
{
    public class WikidataService : IWikidataService
    {
        private readonly IWikidataRestService _wikidataService;

        public WikidataService(IWikidataRestService wikidataService)
        {
            _wikidataService = wikidataService;
        }

        public HumanDto GetDataOnHuman(string id)
        {
            return _wikidataService.GetDataOnHuman(id);
        }
    }
}
