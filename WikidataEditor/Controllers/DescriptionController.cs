using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class DescriptionController : ControllerBase
    {
        private readonly DescriptionService service;

        public DescriptionController(DescriptionService descriptionService)
        {
            service = descriptionService;
        }

        [HttpGet("{id}/descriptions")]
        public IActionResult Get(string id)
        {
            // John Fleming: https://localhost:7085/api/items/Q15429542/descriptions
            return Ok(service.Get(id));
        }

        [HttpGet("description/upsert")]
        public async Task<IActionResult> UpsertDescriptionAsync([FromQuery(Name = "id")] string id, [FromQuery(Name = "description")] string description, [FromQuery(Name = "languagecode")] string languageCode)
        {
            // https://localhost:7085/api/items/description/upsert/?id=Kaboom123&languagecode=en&description=some+description
            await service.UpsertDescription(id, description, languageCode, $"Added/updated {languageCode} description");

            return Ok($"https://www.wikidata.org/wiki/{id}");
        }
    }
}