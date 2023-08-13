using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WikidataEditor.Controllers;
using WikidataEditor.Dtos.CoreData;
using WikidataEditor.Services;

namespace WikidataEditorTests.Controllers
{
    [TestClass]
    public class WikidataItemControllerTests
    {
        [TestMethod]
        public void Get_ShouldReturnOkResult()
        {
            // Arrange
            var coreDataService = new Mock<ICoreDataService>();
            var id = "Q1";

            coreDataService.Setup(x => x.Get(It.IsAny<string>()))
                           .Returns(new FlatWikidataItemDto { Id = id});

            var controller = new CoreDataController(coreDataService.Object);

            var result = controller.Get("some id");

            result.Should().BeOfType<OkObjectResult>();
            var value = ((ObjectResult)result).Value;
            value.Should().BeOfType<FlatWikidataItemDto>();
            ((FlatWikidataItemDto)value).Id.Should().Be(id);
        }
    }
}