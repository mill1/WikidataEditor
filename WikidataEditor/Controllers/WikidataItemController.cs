using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Interfaces;

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
       
        [HttpGet("humans/{id}")]
        public IActionResult GetHumanById(string id)
        {
            /*
                small  https://localhost:7085/api/items/humans/Q99589194 (Lesley Cunliffe)
                medium https://localhost:7085/api/items/humans/Q15429542 (John Fleming)
                large  https://localhost:7085/api/items/humans/Q8016     (Winston Churchill)
            */
            return Ok(_wikidataService.GetDataOnHuman(id));
        }
    }
}