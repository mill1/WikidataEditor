using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/coredata/items")]
    public class WikidataItemController : ControllerBase
    {
        private readonly IWikidataService _wikidataService;

        public WikidataItemController(IWikidataService wikidataService)
        {
            // human: https://localhost:7085/api/coredata/items/?id=Q99589194 (Lesley Cunliffe)
            _wikidataService = wikidataService;
        }

        [HttpGet()]
        public IActionResult Get([FromQuery(Name = "id")] string id)
        {
            return Ok(_wikidataService.GetCoreData(id));
        }

        [HttpGet("{id}")]
        public IActionResult GetCoreData(string id)
        {
            /*
                human:                   https://localhost:7085/api/coredata/items/Q15429542 (John Fleming)
                disambiguation page:     https://localhost:7085/api/coredata/items/Q231486 (Silver)
                astronomical object type https://localhost:7085/api/coredata/items/Q3863 (asteroid)
            */
            return Ok(_wikidataService.GetCoreData(id));
        }
    }
}