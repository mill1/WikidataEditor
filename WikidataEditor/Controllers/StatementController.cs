using Microsoft.AspNetCore.Mvc;
using WikidataEditor.Interfaces;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/statements")]
    public class StatementController : ControllerBase
    {
        private readonly IStatementService _statementService;

        public StatementController(IStatementService statementService)
        {
            _statementService = statementService;
        }
       
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            /*
                small  https://localhost:7085/api/statements/Q99589194 (Lesley Cunliffe)
                medium https://localhost:7085/api/statements/Q15429542 (John Fleming)
                large  https://localhost:7085/api/statements/Q8016     (Winston Churchill)
            */
            return Ok(_statementService.GetWikidataStatements(id));
        }
    }
}