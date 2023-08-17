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
        public void GetCoreData_ShouldReturnOkResult()
        {
            // Arrange
            var itemService = new Mock<IItemService>();
            var id = "Q1";

            itemService.Setup(x => x.GetCoreData(It.IsAny<string>()))
                           .Returns(new FlatWikidataItemDto { Id = id });

            var controller = new ItemController(itemService.Object, null);

            var result = controller.GetCoreData("some id");

            result.Should().BeOfType<OkObjectResult>();
            var value = ((ObjectResult)result).Value;
            value.Should().BeOfType<FlatWikidataItemDto>();
            ((FlatWikidataItemDto)value).Id.Should().Be(id);
        }
    }
}