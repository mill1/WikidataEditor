using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _service;

        public ItemController(IItemService itemService)
        {
            _service = itemService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // John Fleming: https://localhost:7085/api/items/Q15429542
            return Ok(_service.Get(id));
        }

        [HttpGet("{id}/coredata")]
        public IActionResult GetCoreData(string id)
        {
            /*
                human:                    https://localhost:7085/api/items/Q15429542/coredata (John Fleming)
                disambiguation page:      https://localhost:7085/api/items/Q231486/coredata   (Silver)
                astronomical object type: https://localhost:7085/api/items/Q3863/coredata     (asteroid)
                other:                    https://localhost:7085/api/items/Q368481/coredata   (Bonfire (horse))
            */
            return Ok(_service.GetCoreData(id));
        }

        [HttpGet("coredata")]
        public IActionResult GetCoreDataById([FromQuery(Name = "id")] string id)
        {
            // human: https://localhost:7085/api/items/coredata?id=Q99589194 (Lesley Cunliffe)
            return Ok(_service.GetCoreData(id));
        }
    }
}