using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class CoreDataController : ControllerBase
    {
        private readonly ICoreDataService _coreDataService;

        public CoreDataController(ICoreDataService coreDataService)
        {            
            _coreDataService = coreDataService;
        }

        [HttpGet("coredata")]
        public IActionResult GetById([FromQuery(Name = "id")] string id)
        {
            // human: https://localhost:7085/api/items/coredata?id=Q99589194 (Lesley Cunliffe)
            return Ok(_coreDataService.Get(id));
        }

        [HttpGet("{id}/coredata")]
        public IActionResult Get(string id)
        {
            /*
                human:                   https://localhost:7085/api/items/Q15429542/coredata (John Fleming)
                disambiguation page:     https://localhost:7085/api/items/Q231486/coredata   (Silver)
                astronomical object type https://localhost:7085/api/items/Q3863/coredata     (asteroid)
            */
            return Ok(_coreDataService.Get(id));
        }
    }
}