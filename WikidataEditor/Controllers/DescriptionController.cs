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
        public IActionResult Get()
        {
            // TODO
            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> UpsertDescriptionAsync([FromQuery(Name = "id")] string id, [FromQuery(Name = "description")] string description, [FromQuery(Name = "languagecode")] string languageCode)
        {
            await service.UpsertDescription(id, description, languageCode, $"Added/updated {languageCode} description");

            return Ok($"https://www.wikidata.org/wiki/{id}");
        }
    }
}