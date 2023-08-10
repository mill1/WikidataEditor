using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class LabelController : ControllerBase
    {
        private readonly LabelService service;

        public LabelController(LabelService labelService)
        {
            service = labelService;
        }

        [HttpGet("{id}/labels")]
        public async Task<IActionResult> Get(string id)
        {
            // John Fleming: https://localhost:7085/api/items/Q15429542/labels
            return Ok(await service.Get(id));
        }

        [HttpGet("{id}/labels/languagecodes/{languageCode}")]
        public async Task<IActionResult> Get(string id, string languageCode)
        {
            // John Fleming: https://localhost:7085/api/items/Q15429542/labels/languagecodes/en
            return Ok(await service.Get(id, languageCode));
        }

        [HttpGet("labels")]
        public async Task<IActionResult> GetByQueryParameters([FromQuery(Name = "id")] string id, [FromQuery(Name = "languagecode")] string languageCode)
        {
            // Lesley Cunliffe: https://localhost:7085/api/items/labels?id=Q99589194&languagecode=nl
            var result = languageCode == "*" ? await service.Get(id) : await service.Get(id, languageCode);
            return Ok(result);
        }

        [HttpGet("label/upsert")]
        public async Task<IActionResult> UpsertLabelAsync([FromQuery(Name = "id")] string id, [FromQuery(Name = "label")] string label, [FromQuery(Name = "languagecode")] string languageCode)
        {
            // https://localhost:7085/api/items/label/upsert/?id=Kaboom123&languagecode=en&label=some+label
            await service.UpsertLabel(id, label, languageCode, $"Added/updated {languageCode} label");

            return Ok($"https://www.wikidata.org/wiki/{id}");
        }
    }
}