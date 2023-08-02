using FluentAssertions;
using Moq;
using WikidataEditor.Interfaces;
using WikidataEditor.Services;

namespace WikidataEditorTests.Services
{
    [TestClass]
    public class StatementServiceTests
    {
        [TestMethod]
        public void GetWikidataStatements_ShouldReturnDto()
        {
            // Arrange
            var wikidataService = new Mock<IWikidataService>();

            var id = Guid.NewGuid().ToString();

            wikidataService.Setup(x => x.GetStatements(id))
            .Returns(new WikidataEditor.Dtos.WikidataStatementsDto { Id = id, IsHuman = true });

            // Act
            var statementService = new StatementService(wikidataService.Object);
            var dto = statementService.GetWikidataStatements(id);

            // Assert
            Assert.IsNotNull(dto);
            dto.Id.Should().Be(id);
        }
    }
}
