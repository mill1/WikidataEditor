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
        public async Task<IActionResult> Get(string id)
        {
            // John Fleming: https://localhost:44351/api/items/Q15429542/descriptions
            return Ok(await service.Get(id));
        }

        [HttpGet("{id}/descriptions/languagecodes/{languageCode}")]
        public async Task<IActionResult> Get(string id, string languageCode)
        {
            // John Fleming: https://localhost:44351/api/items/Q15429542/descriptions/languagecodes/en
            return Ok(await service.Get(id, languageCode));
        }

        [HttpGet("descriptions")]
        public async Task<IActionResult> GetByQueryParameters([FromQuery(Name = "id")] string id, [FromQuery(Name = "languagecode")] string languageCode)
        {
            // Lesley Cunliffe: https://localhost:44351/api/items/descriptions?id=Q99589194&languagecode=en
            var result = languageCode == "*" ? await service.Get(id) : await service.Get(id, languageCode);
            return Ok(result);
        }

        [HttpGet("description/upsert")]
        public async Task<IActionResult> UpsertDescriptionAsync([FromQuery(Name = "id")] string id, [FromQuery(Name = "description")] string description, [FromQuery(Name = "languagecode")] string languageCode)
        {
            // https://localhost:44351/api/items/description/upsert/?id=Kaboom123&languagecode=en&description=some+description
            await service.UpsertDescription(id, description, languageCode, $"Added/updated {languageCode} description");

            return Ok($"https://www.wikidata.org/wiki/{id}");
        }
    }
}