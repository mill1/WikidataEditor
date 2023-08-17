using Microsoft.AspNetCore.Mvc;
using System.Net;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _service;
        private readonly IWikipediaApiService _wikipediaApiService;

        public ItemController(IItemService itemService, IWikipediaApiService wikipediaApiService)
        {
            _service = itemService;
            _wikipediaApiService = wikipediaApiService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // John Fleming: http://localhost:38583/api/items/Q15429542
            return Ok(_service.Get(id));
        }

        [HttpGet("{id}/coredata")]
        public IActionResult GetCoreData(string id)
        {
            /*
                human:                    http://localhost:38583/api/items/Q15429542/coredata (John Fleming)
                disambiguation page:      http://localhost:38583/api/items/Q231486/coredata   (Silver)
                astronomical object type: http://localhost:38583/api/items/Q3863/coredata     (asteroid)
                other:                    http://localhost:38583/api/items/Q368481/coredata   (Bonfire (horse))
            */
            return Ok(_service.GetCoreData(id));
        }

        [HttpGet]
        public IActionResult GetById([FromQuery(Name = "id")] string id = null, [FromQuery(Name = "wikipediatitle")] string title = null)
        {

            if (id == null && title == null) 
                throw new HttpRequestException("Either field id or title is required", null, HttpStatusCode.BadRequest);

            if(id != null)
                return Ok(_service.Get(id));

            id = _wikipediaApiService.GetWikibaseItemId(title);

            if (id == null)
                throw new HttpRequestException($"No wikidata item found for Wikipedia title '{title}'", null, HttpStatusCode.NotFound);

            return Ok(_service.Get(id));
        }

        [HttpGet("coredata")]
        public IActionResult GetCoreDataById([FromQuery(Name = "id")] string id = null, [FromQuery(Name = "wikipediatitle")] string title = null)
        {

            if (id == null && title == null)
                throw new HttpRequestException("Either field id or title is required", null, HttpStatusCode.BadRequest);

            if (id != null)
                return Ok(_service.GetCoreData(id));

            id = _wikipediaApiService.GetWikibaseItemId(title);

            if (id == null)
                throw new HttpRequestException($"No wikidata item found for Wikipedia title '{title}'", null, HttpStatusCode.NotFound);
            
            return Ok(_service.GetCoreData(id));
        }
    }
}