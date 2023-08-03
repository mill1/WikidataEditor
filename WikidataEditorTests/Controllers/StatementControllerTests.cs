using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WikidataEditor.Controllers;
using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;

namespace WikidataEditorTests.Controllers
{
    [TestClass]
    public class StatementControllerTests
    {
        [TestMethod]
        public void GetWikidataStatements_ShouldReturnOkResult()
        {
            // Arrange
            var statementService = new Mock<IStatementService>();
            var id = "Q1";

            statementService.Setup(x => x.GetWikidataStatements(It.IsAny<string>())).Returns(new HumanDto { Id = id });

            var controller = new StatementController(statementService.Object);

            var result = controller.GetById("some id");

            result.Should().BeOfType<OkObjectResult>();
            var value = ((ObjectResult)result).Value;
            value.Should().BeOfType<HumanDto>();
            ((HumanDto)value).Id.Should().Be(id);
        }
    }
}