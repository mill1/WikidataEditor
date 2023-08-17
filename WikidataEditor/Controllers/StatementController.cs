using Microsoft.AspNetCore.Mvc;
using System.Net;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class StatementController : ControllerBase
    {
        private readonly StatementService _service;
        private readonly IWikipediaApiService _wikipediaApiService;

        public StatementController(StatementService statementService, IWikipediaApiService wikipediaApiService)
        {
            _service = statementService;
            _wikipediaApiService = wikipediaApiService;
        }

        [HttpGet("{id}/statements")]
        public async Task<IActionResult> Get(string id)
        {
            // Lesley Cunliffe: http://localhost:38583/api/items/Q99589194/statements
            return Ok(await _service.Get(id));
        }

        [HttpGet("{id}/statements/property/{property}")]
        public async Task<IActionResult> Get(string id, string property)
        {
            // Bonfire: http://localhost:38583/api/items/Q368481/statements/property/P31
            return Ok(await _service.Get(id, property));
        }

        [HttpGet("statements")]
        public async Task<IActionResult> GetByQueryParameters([FromQuery(Name = "id")] string id, [FromQuery(Name = "property")] string property)
        {
            // Lesley Cunliffe: http://localhost:38583/api/items/statements?id=Q99589194&property=P31
            var result = property == "*" ? await _service.Get(id) : await _service.Get(id, property);
            return Ok(result);
        }

        /// <summary>
        /// Add/Update the statement on Date of death (P570) by adding a url reference
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        [HttpGet("statement/upsertdodref")]
        public IActionResult UpsertStatementDoDReferenceAsync([FromQuery(Name = "title")] string title, [FromQuery(Name = "dateofdeath")] DateOnly dateOfDeath, [FromQuery(Name = "url")] string url)
        {
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                throw new HttpRequestException($"Invalid value field url:  '{url}'", null, HttpStatusCode.BadRequest);

            string id = _wikipediaApiService.GetWikibaseItemId(title);

            if (id == null)
                throw new HttpRequestException($"No wikidata item found for Wikipedia title '{title}'", null, HttpStatusCode.NotFound);

            _service.UpsertStatementDoDReference(id, dateOfDeath, url);

            return Ok($"https://www.wikidata.org/wiki/{id}");
        }
    }
}