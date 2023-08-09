using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using WikidataEditor.Common;
using WikidataEditor.Dtos;
using WikidataEditor.Models;
using WikidataEditor.Models.Instances;
using WikidataEditor.Services;

namespace WikidataEditorTests.Services
{
    [TestClass]
    public class WikidataRestServiceTests
    {
        private const string BaseAddress = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/";

        Mock<IHttpClientFactory> factoryMock;

        [TestInitialize()]
        public void TestInitialize()
        {
            factoryMock = new Mock<IHttpClientFactory>();
        }

        [TestMethod]
        public void GetDataOnHuman_ShouldThrowExceptionIfNotTypeItem()
        {
            // Arrange
            var id = "Q1";
            var handlerMock = new MockHttpMessageHandler();

            // Setup response
            string jsonString = @"{""type"":""someothertype"",""id"":""Q1""}";

            handlerMock
                .When(BaseAddress + "items/" + id)
                .Respond("application/json", jsonString);

            // Act
            var httpClient = new HttpClient(handlerMock);
            httpClient.BaseAddress = new Uri(BaseAddress);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new WikidataRestService(factoryMock.Object, null, null);

            // Assert
            service.Invoking(y => y.GetCoreData(id))
            .Should().Throw<ArgumentException>()
            .WithMessage("Result is not of type item. Encountered type: someothertype");
        }

        [TestMethod]
        public void GetDataOnHuman_ShouldReturnEmptyObjectIfNotIsHuman()
        {
            // Arrange
            const string idNonHuman = "Q368481";

            var baseData = new WikidataItemBaseDto();
            baseData.Id = idNonHuman;
            baseData.Label = "horse";
            baseData.Description = "horse";
            baseData.StatementsCount = 1;
            baseData.InstanceOf = new List<string> { "horse (Q726)" };
            baseData.Aliases = new List<string> { "Gestion Bonfire" };

            var expected = new WikidataItemOtherDto(baseData)
            {
                UriCollection = new UriCollectionDto
                {
                    WikidataUri = "https://www.wikidata.org/wiki/" + idNonHuman,
                    Wikipedias = new List<string> { "*no values*" },
                    InstanceUris = null
                }
            };

            var helperMock = new Mock<IWikidataHelper>();
            helperMock.Setup(x => x.GetTextValue(It.IsAny<LanguageCodes>()))
            .Returns("horse");
            helperMock.Setup(x => x.ResolveValue(It.IsAny<Statement[]>()))
            .Returns(new List<string> { "horse" });

            string jsonString = @"{""type"":""item"",""labels"":{""en"":""horse"",""nl"":""paard""},""descriptions"":{""en"":""horse"",""nl"":""renpaard""},""aliases"":{""en"":[""Gestion Bonfire""]},""statements"":{""P31"":[{""id"":""Q368481"",""value"":{""type"":""value"",""content"":""Q726""}}]},""sitelinks"":{},""id"":""Q368481""}";

            var handlerMock = new MockHttpMessageHandler();

            // Setup responses
            handlerMock
                .When(BaseAddress + "items/" + idNonHuman)
                .Respond("application/json", jsonString);
            handlerMock
                .When(BaseAddress + "items/Q726" + @"/labels")
                .Respond("application/json", @"{""af"":""perd"",""en"":""horse"",""gl"":""Cabalo""}");

            // Act
            var httpClient = new HttpClient(handlerMock);
            httpClient.BaseAddress = new Uri(BaseAddress);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var service = new WikidataRestService(factoryMock.Object, null, helperMock.Object);

            var actual = service.GetCoreData(idNonHuman);

            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetDataOnHuman_ShouldReturnMinimalData()
        {
            // Arrange            

            var id = "Q99589194";

            var missing = new List<string> { Constants.Missing };

            var baseData = new WikidataItemBaseDto();
            baseData.Id = id;
            baseData.Label = Constants.Missing;
            baseData.Description = Constants.Missing;
            baseData.StatementsCount = 1;
            baseData.InstanceOf = new List<string> { "human (Q5)" };
            baseData.Aliases = missing;

            var expected = new WikidataItemHumanDto(baseData)
            {
                SexOrGender = missing,
                CountryOfCitizenship = missing,
                GivenName = missing,
                FamilyName = missing,
                DateOfBirth = missing,
                PlaceOfBirth = missing,
                DateOfDeath = missing,
                PlaceOfDeath = missing,
                Occupation = missing,
                UriCollection = new UriCollectionDto
                {
                    WikidataUri = "https://www.wikidata.org/wiki/" + id,
                    Wikipedias = new List<string> { "*no values*" },
                    InstanceUris = new List<string> { "*no values*" }
                }
            };

            var helperMock = new Mock<IWikidataHelper>();
            helperMock.Setup(x => x.GetTextValue(It.IsAny<LanguageCodes>()))
            .Returns("some value");
            helperMock.Setup(x => x.ResolveValue(It.IsAny<Statement[]>()))
            .Returns(new List<string> { "Q5" });

            var mappingServiceMock = new Mock<IMappingService>();
            mappingServiceMock.Setup(x => x.MapToHumanDto(It.IsAny<WikidataItemBaseDto>(), It.IsAny<WikidataItemOnHumans>()))
                .Returns(expected);

            var handlerMock = new MockHttpMessageHandler();

            // Setup responses
            string jsonString = @"{""type"":""item"",""labels"":{},""descriptions"":{},""aliases"":{},""statements"":{""P31"":[{""id"":""Q99589194"",""value"":{""type"":""value"",""content"":""Q5""}}]},""sitelinks"":{},""id"":""Q99589194""}";
            handlerMock
                .When(BaseAddress + "items/" + Constants.WikidataIdHuman + @"/labels")
                .Respond("application/json", @"{""af"":""mens"",""en"":""human"",""nn"":""menneske""}");
            handlerMock
                .When(BaseAddress + "items/" + id)
                .Respond("application/json", jsonString);

            // Act
            var httpClient = new HttpClient(handlerMock);
            httpClient.BaseAddress = new Uri(BaseAddress);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var service = new WikidataRestService(factoryMock.Object, mappingServiceMock.Object, helperMock.Object);
            var actual = service.GetCoreData(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetDataOnHuman_ShouldReturnFirstFilledMainLanguageIfEnglishIsNotFound()
        {
            // Arrange
            const string id = "Q99589194";

            var helperMock = new Mock<IWikidataHelper>();
            helperMock.Setup(x => x.GetTextValue(It.IsAny<LanguageCodes>()))
            .Returns("some value");
            helperMock.Setup(x => x.ResolveValue(It.IsAny<Statement[]>()))
            .Returns(new List<string> { "Q5" });

            var mappingServiceMock = new Mock<IMappingService>();
            mappingServiceMock.Setup(x => x.MapToHumanDto(It.IsAny<WikidataItemBaseDto>(), It.IsAny<WikidataItemOnHumans>()))
                .Returns(new WikidataItemHumanDto(new WikidataItemBaseDto()) { Label = "Dutch label" });

            string jsonString = @"{""type"":""item"",""labels"":{""af"":""Afrikaans label"",""nl"":""Dutch label"",""no"":""Norwegian label""},""descriptions"":{},""aliases"":{},""statements"":{""P31"":[{""id"":""Q99589194"",""value"":{""type"":""value"",""content"":""Q5""}}]},""sitelinks"":{},""id"":""Q99589194""}";

            var handlerMock = new MockHttpMessageHandler();

            // Setup responses
            handlerMock
                .When(BaseAddress + "items/" + id)
                .Respond("application/json", jsonString);
            handlerMock
                .When(BaseAddress + "items/" + Constants.WikidataIdHuman + @"/labels")
                .Respond("application/json", @"{""af"":""mens"",""en"":""human"",""nn"":""menneske""}");

            // Act
            var httpClient = new HttpClient(handlerMock);
            httpClient.BaseAddress = new Uri(BaseAddress);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var service = new WikidataRestService(factoryMock.Object, mappingServiceMock.Object, helperMock.Object);

            var actual = service.GetCoreData(id);

            actual.Label.Should().Be("Dutch label");
        }

        [TestMethod]
        public void GetDataOnHuman_ShouldReturnFirstLabelIfAllMainLanguagesAreEmpty()
        {
            // Arrange
            const string id = "Q99589194";

            var helperMock = new Mock<IWikidataHelper>();
            helperMock.Setup(x => x.GetTextValue(It.IsAny<LanguageCodes>()))
            .Returns("some value");
            helperMock.Setup(x => x.ResolveValue(It.IsAny<Statement[]>()))
            .Returns(new List<string> { "Q5" });

            var mappingServiceMock = new Mock<IMappingService>();
            mappingServiceMock.Setup(x => x.MapToHumanDto(It.IsAny<WikidataItemBaseDto>(), It.IsAny<WikidataItemOnHumans>()))
                .Returns(new WikidataItemHumanDto(new WikidataItemBaseDto()) { Label = "Afrikaans label" });

            string jsonString = @"{""type"":""item"",""labels"":{""af"":""Afrikaans label"",""no"":""Norwegian label""},""descriptions"":{},""aliases"":{},""statements"":{""P31"":[{""id"":""Q99589194"",""value"":{""type"":""value"",""content"":""Q5""}}]},""sitelinks"":{},""id"":""Q99589194""}";

            var handlerMock = new MockHttpMessageHandler();

            // Setup responses
            handlerMock
                .When(BaseAddress + "items/" + id)
                .Respond("application/json", jsonString);
            handlerMock
                .When(BaseAddress + "items/" + Constants.WikidataIdHuman + @"/labels")
                .Respond("application/json", @"{""af"":""mens"",""en"":""human"",""nn"":""menneske""}");

            // Act
            var httpClient = new HttpClient(handlerMock);
            httpClient.BaseAddress = new Uri(BaseAddress);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var service = new WikidataRestService(factoryMock.Object, mappingServiceMock.Object, helperMock.Object);

            var actual = service.GetCoreData(id);

            // Assert
            actual.Label.Should().Be("Afrikaans label");
        }

        [TestMethod]
        public void GetDataOnHuman_ShouldReturnData()
        {
            // Arrange
            string jsonString = GetJsonString();
            var id = "Q99589194";

            var baseData = new WikidataItemBaseDto();
            baseData.Id = id;
            baseData.Label = "Lesley Cunliffe";
            baseData.Description = "American journalist and writer";
            baseData.StatementsCount = 14;
            baseData.InstanceOf = new List<string> { "human (Q5)" };
            baseData.Aliases = new List<string> { "Lesley Hume Cunliffe", "Hume" };

            var expected = new WikidataItemHumanDto(baseData)
            {
                SexOrGender = new List<string> { "female" },
                CountryOfCitizenship = new List<string> { Constants.Missing },
                GivenName = new List<string> { "Lesley" },
                FamilyName = new List<string> { "Cunliffe" },
                DateOfBirth = new List<string> { "+1945-05-21T00:00:00Z" },
                PlaceOfBirth = new List<string> { Constants.Missing },
                DateOfDeath = new List<string> { "+1997-03-28T00:00:00Z" },
                PlaceOfDeath = new List<string> { Constants.Missing },
                Occupation = new List<string> { "journalist", "writer", "editor" },
                UriCollection = new UriCollectionDto
                {
                    WikidataUri = "https://www.wikidata.org/wiki/" + id,
                    Wikipedias = new List<string> { "https://en.wikipedia.org/wiki/Lesley_Cunliffe" },
                    InstanceUris = new List<string> { "https://id.loc.gov/authorities/names/n81098631.html" }
                }
            };

            var helperMock = new Mock<IWikidataHelper>();
            helperMock.Setup(x => x.GetTextValue(It.IsAny<LanguageCodes>()))
            .Returns("some value");
            helperMock.Setup(x => x.ResolveValue(It.IsAny<Statement[]>()))
            .Returns(new List<string> { "Q5" });

            var mappingServiceMock = new Mock<IMappingService>();
            mappingServiceMock.Setup(x => x.MapToHumanDto(It.IsAny<WikidataItemBaseDto>(), It.IsAny<WikidataItemOnHumans>()))
                .Returns(expected);

            var handlerMock = new MockHttpMessageHandler();

            // Setup various responses
            handlerMock
                .When(BaseAddress + "items/" + id)
                .Respond("application/json", jsonString);
            handlerMock
                .When(BaseAddress + "items/" + Constants.WikidataIdHuman + @"/labels")
                .Respond("application/json", @"{""af"":""mens"",""en"":""human"",""nn"":""menneske""}");
            handlerMock
                .When(BaseAddress + "items/Q6581072" + @"/labels") // https://www.wikidata.org/wiki/Q6581072 : to be used in "sex or gender" (P21)
                .Respond("application/json", @"{""af"":""vroulik"", ""en"":""female"",""zu"":""isifazane""}");
            handlerMock
                .When(BaseAddress + "items/Q18658557" + @"/labels")
                .Respond("application/json", @"{""af"":""Lesley"",""en"":""Lesley"",""zu"":""Lesley""}");
            handlerMock
                .When(BaseAddress + "items/Q21493284" + @"/labels")
                .Respond("application/json", @"{""af"":""Cunliffe"",""en"":""Cunliffe"",""zu"":""Cunliffe""}");
            handlerMock
                .When(BaseAddress + "items/Q1930187" + @"/labels")
                .Respond("application/json", @"{""af"":""joernalis"",""en"":""journalist"",""zu"":""intatheli""}");
            handlerMock
                .When(BaseAddress + "items/Q36180" + @"/labels")
                .Respond("application/json", @"{""af"":""skrywer"",""en"":""writer"",""zu"":""umbhali""}");
            handlerMock
                .When(BaseAddress + "items/Q1607826" + @"/labels")
                .Respond("application/json", @"{""ak"":""Samufo"",""en"":""editor"", ""zh-tw"":""\u7de8\u8f2f""}");

            // Act
            var httpClient = new HttpClient(handlerMock);
            httpClient.BaseAddress = new Uri(BaseAddress);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var service = new WikidataRestService(factoryMock.Object, mappingServiceMock.Object, helperMock.Object);

            var actual = service.GetCoreData(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        private static string GetJsonString()
        {
            // TOD: sitelinks: list of all wikis in model (see LanguageCodes.cs): enwiki, itwiki -> url naar wikipedia article?
            // Wikidata: https://www.wikidata.org/wiki/Q99589194 (Lesley Cunliffe)
            // As JSON:  https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/Q99589194. Not a minimal test, I know.
            return @"{""type"":""item"",""labels"":{""en"":""Lesley Cunliffe"",""nl"":""Lesley Cunliffe"",""sq"":""Lesley Cunliffe""},""descriptions"":{""en"":""American journalist and writer"",""nl"":""Amerikaanse journalist en schrijfster""},""aliases"":{""en"":[""Lesley Hume Cunliffe"",""Hume""]},""statements"":{""P31"":[{""id"":""Q99589194$baecb170-4527-bd14-9e5f-2085da6957b4"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P31"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q5""}}],""P21"":[{""id"":""Q99589194$4dee092d-475c-ebbb-68a6-c02c6835831e"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P21"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q6581072""}}],""P7859"":[{""id"":""Q99589194$6bc60fa4-49c3-a9d2-0c04-e7f890a37790"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P7859"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""lccn-n81098631""}}],""P214"":[{""id"":""Q99589194$cedb4a3c-4af4-8d73-4727-b1c84be3a639"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P214"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""43164717""}}],""P244"":[{""id"":""Q99589194$490595d5-4390-2055-a3fb-2f2d82d53d5a"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P244"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""n81098631""}}],""P213"":[{""id"":""Q99589194$89974327-48f7-eac6-553c-5e7248f0b393"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P213"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""0000 0001 1630 2693""}}],""P1207"":[{""id"":""Q99589194$0cde1823-4ae3-200a-7f20-565cf378298f"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P1207"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""n01052642""}}],""P569"":[{""id"":""Q99589194$3563c8a4-ffba-48f6-bbdd-cc2732effeac"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P569"",""data-type"":""time""},""value"":{""type"":""value"",""content"":{""time"":""+1945-05-21T00:00:00Z"",""precision"":11,""calendarmodel"":""http://www.wikidata.org/entity/Q1985727""}}}],""P570"":[{""id"":""Q99589194$735047aa-8390-4d3d-b434-77cdb2d84fa0"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P570"",""data-type"":""time""},""value"":{""type"":""value"",""content"":{""time"":""+1997-03-28T00:00:00Z"",""precision"":11,""calendarmodel"":""http://www.wikidata.org/entity/Q1985727""}}}],""P106"":[{""id"":""Q99589194$c9eaeafe-503d-40e8-89e0-b4238117ec67"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P106"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q1930187""}},{""id"":""Q99589194$579494ec-2a63-432a-baf9-91eeb3aceeac"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P106"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q36180""}},{""id"":""Q99589194$184ae3b9-3a76-426e-b624-a03bc2f52a04"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""fa278ebfc458360e5aed63d5058cca83c46134f1"",""parts"":[{""property"":{""id"":""P143"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q328""}}]}],""property"":{""id"":""P106"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q1607826""}}],""P2671"":[{""id"":""Q99589194$CD44F365-94CB-463E-BD05-1DE1C6127E88"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P2671"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""/g/11f5038261""}}],""P735"":[{""id"":""Q99589194$88A20C14-6CC4-412D-BF79-0B58B70D2C74"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P735"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q18658557""}}],""P8189"":[{""id"":""Q99589194$179C62D3-D8ED-47B5-A258-0326DF7BA5F1"",""rank"":""normal"",""qualifiers"":[],""references"":[{""hash"":""ecb7a39cf436eb1fba419853af168ef9f2ba1dab"",""parts"":[{""property"":{""id"":""P248"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q106509962""}}]}],""property"":{""id"":""P8189"",""data-type"":""external-id""},""value"":{""type"":""value"",""content"":""987007436076205171""}}],""P734"":[{""id"":""Q99589194$273AB5CE-7711-4986-A0E7-29A016946CCC"",""rank"":""normal"",""qualifiers"":[],""references"":[],""property"":{""id"":""P734"",""data-type"":""wikibase-item""},""value"":{""type"":""value"",""content"":""Q21493284""}}]},""sitelinks"":{""enwiki"":{""title"":""Lesley Cunliffe"",""badges"":[""Q17437798""],""url"":""https://en.wikipedia.org/wiki/Lesley_Cunliffe""}},""id"":""Q99589194""}";
        }
    }
}