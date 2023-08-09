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
            var coreDataServiceMock = new Mock<ICoreDataService>();

            var id = Guid.NewGuid().ToString();

            var expected = new WikidataItemHumanDto(new WikidataItemBaseDto { Id = id });

            coreDataServiceMock.Setup(x => x.Get(id))
            .Returns(expected);

            // Act
            var service = new WikidataService(coreDataServiceMock.Object);
            var actual = service.GetCoreData(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
