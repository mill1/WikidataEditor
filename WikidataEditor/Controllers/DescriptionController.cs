using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/descriptions")]
    public class DescriptionController : ControllerBase
    {
        private readonly DescriptionService service;

        public DescriptionController(DescriptionService descriptionService)
        {
            service = descriptionService;
        }

        [HttpGet()]
        public async Task<IActionResult> UpdateDescriptionAsync([FromQuery(Name = "id")] string id, [FromQuery(Name = "description")] string description)
        {
            await service.UpdateDescription(id, description);



            return Ok($"https://www.wikidata.org/wiki/{id}");
        }
    }
}