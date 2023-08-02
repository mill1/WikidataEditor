using FluentAssertions;
using Moq;
using WikidataEditor.Dtos;
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

            var expected  = new WikidataStatementsDto { Id = id, Label = $"Label{id}", IsHuman = true };

            wikidataService.Setup(x => x.GetStatements(id))
            .Returns(expected);

            // Act
            var statementService = new StatementService(wikidataService.Object);
            var actual = statementService.GetWikidataStatements(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
