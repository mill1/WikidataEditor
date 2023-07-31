using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using WikidataEditor.Dtos;
using WikidataEditor.Models;
using WikidataEditor.Services;

namespace WikidataEditor.Controllers
{
    [ApiController]
    [Route("api/statements")]
    public class StatementController : ControllerBase
    {
        private readonly StatementService _statementService;
        private readonly ILogger<StatementController> _logger;

        public StatementController(StatementService statementService, ILogger<StatementController> logger)
        {
            _statementService = statementService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                // https://localhost:7085/api/statements/Q15429542  (John Fleming)
                return Ok(_statementService.GetWikidataStatements(id));
            }
            catch (AggregateException e)
            {
                var statusCode = ((HttpRequestException)e?.InnerException)?.StatusCode;
                if (statusCode == HttpStatusCode.BadRequest) 
                { 
                    return BadRequest();
                }
                return Problem();
            }
            catch 
            {
                return Problem();
            }
        }
    }
}