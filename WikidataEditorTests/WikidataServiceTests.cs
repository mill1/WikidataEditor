using FluentAssertions;
using RichardSzalay.MockHttp;
using WikidataEditor.Dtos;
using WikidataEditor.Services;

namespace WikidataEditorTests
{
    [TestClass]
    public class WikidataServiceTests
    {
        [TestMethod]
        public void GetStatements_ShouldReturnEmptyObjectIfNotIsHuman()
        {
            // Arrange
            const string idNonHuman = "Q368481";

            var expected = new WikidataStatementsDto { Id = idNonHuman, IsHuman = false };

            string jsonString = @"{""P31"":[{""id"":""Q368481"",""value"":{""type"":""value"",""content"":""Q726""}}]}";

            var handlerMock = new MockHttpMessageHandler();
            var urlBase = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/";

            // Setup response
            handlerMock
                .When(urlBase + idNonHuman + @"/statements")
                .Respond("application/json", jsonString);

            // Act
            var httpClient = new HttpClient(handlerMock);
            var service = new WikidataService(httpClient);

            var actual = service.GetStatements(idNonHuman);

            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetStatements_ShouldReturnMinimalStatements()
        {
            // Arrange
            const string Missing = "*missing*";

            var id = "Q99589194";

            var missing = new List<string> { Missing };

            var expected = new WikidataStatementsDto
            {
                Id = id,
                Label = Missing,
                IsHuman = true,
                SexOrGender = missing,
                CountryOfCitizenship = missing,
                GivenName =  missing,
                FamilyName =  missing,
                DateOfBirth = missing,
                PlaceOfBirth = missing,
                DateOfDeath = missing,
                PlaceOfDeath = missing,
                Occupation = missing,
            };

            var handlerMock = new MockHttpMessageHandler();
            var urlBase = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/";

            /* Not used: HttpMessageHandler cannot setup different requests
            var response = new HttpResponseMessage
            {StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonString)};              
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);
            */

            // Setup various responses
            string jsonString = @"{""P31"":[{""id"":""Q99589194"",""value"":{""type"":""value"",""content"":""Q5""}}]}";

            handlerMock
                .When(urlBase + id + @"/statements")
                .Respond("application/json", jsonString);

            handlerMock
                .When(urlBase + id + @"/labels")
                .Respond("application/json", "{}");

            // Act
            var httpClient = new HttpClient(handlerMock);
            var service = new WikidataService(httpClient);
            var actual = service.GetStatements(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetStatements_ShouldReturnFirstLabelIfEnglishIsNotFound()
        {
            // Arrange
            const string id = "Q99589194";

            string jsonString = @"{""P31"":[{""id"":""Q368481"",""value"":{""type"":""value"",""content"":""Q5""}}]}";

            var handlerMock = new MockHttpMessageHandler();
            var urlBase = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/";

            // Setup various responses
            handlerMock
                .When(urlBase + id + @"/statements")
                .Respond("application/json", jsonString);
            handlerMock
                .When(urlBase + id + @"/labels")
                .Respond("application/json", @"{""nl"":""Dutch label"",""no"":""Norwegian label""}");

            // Act
            var httpClient = new HttpClient(handlerMock);
            var service = new WikidataService(httpClient);

            var actual = service.GetStatements(id);

            actual.Label.Should().Be("Dutch label");
        }

        [TestMethod]
        public void GetStatements_ShouldReturnStatements()
        {
            // Arrange
            string jsonString = GetJsonString();

            var id = "Q99589194";
            var expected = new WikidataStatementsDto
            {
                Id = id,
                Label = "Lesley Cunliffe",
                IsHuman = true,
                SexOrGender = new List<string> { "female" },
                CountryOfCitizenship = new List<string> { "*missing*" },
                GivenName = new List<string> { "Lesley" },
                FamilyName = new List<string> { "Cunliffe" },
                DateOfBirth = new List<string> { "+1945-05-21T00:00:00Z" },
                PlaceOfBirth = new List<string> { "*missing*" },
                DateOfDeath = new List<string> { "+1997-03-28T00:00:00Z" },
                PlaceOfDeath = new List<string> { "*missing*" },
                Occupation = new List<string> { "journalist", "writer", "editor" },
            };

            var handlerMock = new MockHttpMessageHandler();
            var urlBase = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/";

            // Setup various responses
            handlerMock
                .When(urlBase + id + @"/statements")
                .Respond("application/json", jsonString);
            handlerMock
                .When(urlBase + id + @"/labels")
                .Respond("application/json", @"{""en"":""Lesley Cunliffe"",""nl"":""Lesley Cunliffe"",""sq"":""Lesley Cunliffe""}");
            handlerMock
                .When(urlBase + "Q6581072" + @"/labels") // https://www.wikidata.org/wiki/Q6581072 : to be used in "sex or gender" (P21)
                .Respond("application/json", @"{""af"":""vroulik"", ""en"":""female"",""zu"":""isifazane""}");
            handlerMock
                .When(urlBase + "Q18658557" + @"/labels")
                .Respond("application/json", @"{""af"":""Lesley"",""en"":""Lesley"",""zu"":""Lesley""}");
            handlerMock
                .When(urlBase + "Q21493284" + @"/labels")
                .Respond("application/json", @"{""af"":""Cunliffe"",""en"":""Cunliffe"",""zu"":""Cunliffe""}");
            handlerMock
                .When(urlBase + "Q1930187" + @"/labels")
                .Respond("application/json", @"{""af"":""joernalis"",""en"":""journalist"",""zu"":""intatheli""}");
            handlerMock
                .When(urlBase + "Q36180" + @"/labels")
                .Respond("application/json", @"{""af"":""skrywer"",""en"":""writer"",""zu"":""umbhali""}");
            handlerMock
                .When(urlBase + "Q1607826" + @"/labels")
                .Respond("application/json", @"{""ak"":""Samufo"",""en"":""editor"", ""zh-tw"":""\u7de8\u8f2f""}");

            // Act
            var httpClient = new HttpClient(handlerMock);
            var service = new WikidataService(httpClient);

            var actual = service.GetStatements(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        private static string GetJsonString()
        {            
            // Wikidata: https://www.wikidata.org/wiki/Q99589194 (Lesley Cunliffe)
            // As JSON:  https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/Q99589194/statements. Not a minimal test, I know.
            return @"{""P31"":[{""id"":""Q99589194$baecb170-4527-bd14-9e5f-2085da6957b4"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P31"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q5""}}],""P21"":[{""id"":""Q99589194$4dee092d-475c-ebbb-68a6-c02c6835831e"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P21"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q6581072""}}],""P7859"":[{""id"":""Q99589194$6bc60fa4-49c3-a9d2-0c04-e7f890a37790"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P7859"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""lccn-n81098631""}}],""P214"":[{""id"":""Q99589194$cedb4a3c-4af4-8d73-4727-b1c84be3a639"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P214"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""43164717""}}],""P244"":[{""id"":""Q99589194$490595d5-4390-2055-a3fb-2f2d82d53d5a"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P244"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""n81098631""}}],""P213"":[{""id"":""Q99589194$89974327-48f7-eac6-553c-5e7248f0b393"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P213"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""0000 0001 1630 2693""}}],""P1207"":[{""id"":""Q99589194$0cde1823-4ae3-200a-7f20-565cf378298f"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P1207"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""n01052642""}}],""P569"":[{""id"":""Q99589194$3563c8a4-ffba-48f6-bbdd-cc2732effeac"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P569"",""data-type"":""time""},""value"":{""type"":""value"",""content"":{""time"":""+1945-05-21T00:00:00Z"",""precision"":11,""calendarmodel"":""http:\/\/www.wikidata.org\/entity\/Q1985727""}}}],""P570"":[{""id"":""Q99589194$735047aa-8390-4d3d-b434-77cdb2d84fa0"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P570"",""data-type"":""time""},""value"":{""type"":""value"",""content"":{""time"":""+1997-03-28T00:00:00Z"",""precision"":11,""calendarmodel"":""http:\/\/www.wikidata.org\/entity\/Q1985727""}}}],""P106"":[{""id"":""Q99589194$c9eaeafe-503d-40e8-89e0-b4238117ec67"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P106"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q1930187""}},{""id"":""Q99589194$579494ec-2a63-432a-baf9-91eeb3aceeac"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P106"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q36180""}},{""id"":""Q99589194$184ae3b9-3a76-426e-b624-a03bc2f52a04"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P106"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q1607826""}}],""P2671"":[{""id"":""Q99589194$CD44F365-94CB-463E-BD05-1DE1C6127E88"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P2671"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""\/g\/11f5038261""}}],""P735"":[{""id"":""Q99589194$88A20C14-6CC4-412D-BF79-0B58B70D2C74"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P735"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q18658557""}}],""P8189"":[{""id"":""Q99589194$179C62D3-D8ED-47B5-A258-0326DF7BA5F1"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""ecb7a39cf436eb1fba419853af168ef9f2ba1dab"",""parts"":[{""property"":{""id"":""P248"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q106509962""}}]}],""property"":{""id"":""P8189"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""987007436076205171""}}],""P734"":[{""id"":""Q99589194$273AB5CE-7711-4986-A0E7-29A016946CCC"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P734"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q21493284""}}]}";
        }
    }
}