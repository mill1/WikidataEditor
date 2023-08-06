using FluentAssertions;
using Moq;
using WikidataEditor.Dtos;
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
            var wikidataRestServiceMock = new Mock<IWikidataRestService>();

            var id = Guid.NewGuid().ToString();

            var expected  = new WikidataItemHumanDto(new WikidataItemBaseDto { Id  = id });

            wikidataRestServiceMock.Setup(x => x.GetData(id))
            .Returns(expected);

            // Act
            var service = new WikidataService(wikidataRestServiceMock.Object);
            var actual = service.GetDataOnHuman(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
