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

        [HttpGet("label/upsert")]
        public async Task<IActionResult> UpsertLabelAsync([FromQuery(Name = "id")] string id, [FromQuery(Name = "label")] string label, [FromQuery(Name = "languagecode")] string languageCode)
        {
            // https://localhost:7085/api/items/label/upsert/?id=Kaboom123&languagecode=en&label=some+label
            await service.UpsertLabel(id, label, languageCode, $"Added/updated {languageCode} label");

            return Ok($"https://www.wikidata.org/wiki/{id}");
        }
    }
}