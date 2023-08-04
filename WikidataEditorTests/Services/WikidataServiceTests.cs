using FluentAssertions;
using Moq;
using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;
using WikidataEditor.Services;

namespace WikidataEditorTests.Services
{
    [TestClass]
    public class WikidataServiceTests
    {
        [TestMethod]
        public void GetDataOnHuman_ShouldReturnDto()
        {
            // Arrange
            var wikidataRestService = new Mock<IWikidataRestService>();

            var id = Guid.NewGuid().ToString();

            var expected = new WikidataItemHumanDto(new WikidataItemBaseDto { Id = id });

            wikidataRestService.Setup(x => x.GetDataOnHuman(id))
            .Returns(expected);

            // Act
            var service = new WikidataService(wikidataRestService.Object);
            var actual = service.GetDataOnHuman(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
