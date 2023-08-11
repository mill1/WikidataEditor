using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class AliasesController : ControllerBase
    {
        private readonly AliasesService service;

        public AliasesController(AliasesService aliasesService)
        {
            service = aliasesService;
        }

        [HttpGet("{id}/aliases")]
        public async Task<IActionResult> Get(string id)
        {
            //  Douglas Adams: https://localhost:7085/api/items/Q42/aliases
            return Ok(await service.Get(id));
        }

        [HttpGet("{id}/aliases/languagecodes/{languageCode}")]
        public async Task<IActionResult> Get(string id, string languageCode)
        {
            // Douglas Adams: https://localhost:7085/api/items/Q42/aliases/languagecodes/en
            return Ok(await service.Get(id, languageCode));
        }

        [HttpGet("aliases")]
        public async Task<IActionResult> GetByQueryParameters([FromQuery(Name = "id")] string id, [FromQuery(Name = "languagecode")] string languageCode)
        {
            // Douglas Adams: https://localhost:7085/api/items/aliases?id=Q42&languagecode=nl
            var result = languageCode == "*" ? await service.Get(id) : await service.Get(id, languageCode);
            return Ok(result);
        }
    }
}