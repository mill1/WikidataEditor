using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Models;
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
            // John Fleming: https://localhost:7085/api/items/Q15429542/descriptions
            return Ok(await service.Get(id));
        }

        [HttpGet("{id}/descriptions/languagecode/{languageCode}")]
        public async Task<IActionResult> GetByLanguageCode(string id, string languageCode)
        {
            // John Fleming: https://localhost:7085/api/items/Q15429542/descriptions/languagecode/en
            // TODO
            throw new NotImplementedException();
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