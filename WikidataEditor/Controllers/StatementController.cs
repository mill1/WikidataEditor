using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class StatementController : ControllerBase
    {
        private readonly StatementService _service;

        public StatementController(StatementService statementService)
        {
            _service = statementService;
        }

        [HttpGet("{id}/statements")]
        public async Task<IActionResult> Get(string id)
        {
            // Lesley Cunliffe: https://localhost:44351/api/items/Q99589194/statements
            return Ok(await _service.Get(id));
        }

        [HttpGet("{id}/statements/property/{property}")]
        public async Task<IActionResult> Get(string id, string property)
        {
            // Bonfire: https://localhost:44351/api/items/Q368481/statements/property/P31
            return Ok(await _service.Get(id, property));
        }

        [HttpGet("statements")]
        public async Task<IActionResult> GetByQueryParameters([FromQuery(Name = "id")] string id, [FromQuery(Name = "property")] string property)
        {
            // Lesley Cunliffe: https://localhost:44351/api/items/statements?id=Q99589194&property=P31
            var result = property == "*" ? await _service.Get(id) : await _service.Get(id, property);
            return Ok(result);
        }
    }
}