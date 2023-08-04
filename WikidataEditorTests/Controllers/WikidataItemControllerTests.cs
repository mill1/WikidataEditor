using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WikidataEditor.Controllers;
using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;

namespace WikidataEditorTests.Controllers
{
    [TestClass]
    public class WikidataItemControllerTests
    {
        [TestMethod]
        public void GetHumanById_ShouldReturnOkResult()
        {
            // Arrange
            var wikidataService = new Mock<IWikidataService>();
            var id = "Q1";

            wikidataService.Setup(x => x.GetDataOnHuman(It.IsAny<string>())).Returns(new WikidataItemHumanDto { Id = id });

            var controller = new WikidataItemController(wikidataService.Object);

            var result = controller.GetHumanById("some id");

            result.Should().BeOfType<OkObjectResult>();
            var value = ((ObjectResult)result).Value;
            value.Should().BeOfType<WikidataItemHumanDto>();
            ((WikidataItemHumanDto)value).Id.Should().Be(id);
        }
    }
}