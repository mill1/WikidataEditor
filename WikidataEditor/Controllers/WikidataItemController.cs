using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class WikidataItemController : ControllerBase
    {
        private readonly IWikidataService _wikidataService;

        public WikidataItemController(IWikidataService wikidataService)
        {
            _wikidataService = wikidataService;
        }

        [HttpGet("{id}")]
        public IActionResult GetId(string id)
        {
            /*
                human:                   https://localhost:7085/api/items/Q15429542 (John Fleming)
                disambiguation page:     https://localhost:7085/api/items/Q231486 (Silver)
                astronomical object type https://localhost:7085/api/items/Q3863 (asteroid)
            */
            return Ok(_wikidataService.GetDataOnHuman(id));
        }
    }
}