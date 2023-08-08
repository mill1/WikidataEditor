using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using WikidataEditor.Models;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/descriptions")]
    public class DescriptionController : ControllerBase
    {
        [HttpGet()]
        public IActionResult UpdateDescription([FromQuery(Name = "id")] string id, [FromQuery(Name = "description")] string description)
        {
            var url = $"https://www.wikidata.org/wiki/{id}";

            return Ok(url);
        }
    }
}