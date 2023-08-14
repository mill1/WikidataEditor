using Newtonsoft.Json.Linq;
using WikidataEditor.Common;
using WikidataEditor.Configuration;
using WikidataEditor.Dtos.CoreData;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class CoreDataService : ICoreDataService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;
        private readonly IWikidataHelper _helper;

        public CoreDataService(IConfiguration configuration, IHttpClientWikidataApi httpClientWikidataApi, IWikidataHelper wikidataHelper)
        {
            _configuration = configuration;
            _httpClientWikidataApi = httpClientWikidataApi;
            _helper = wikidataHelper;

            var positionOptions = new PositionOptions();
            _configuration.GetSection(PositionOptions.Position).Bind(positionOptions);
        }

        public FlatWikidataItemDto Get(string id)
        {
            var jsonString = _httpClientWikidataApi.GetStringAsync("items/" + id).Result;

            JObject jObject = JObject.Parse(jsonString);

            var type = (string)((JValue)jObject["type"])?.Value ?? "null";
            if (type != "item")
                throw new ArgumentException($"Result is not of type item. Encountered type: {type}");

            JObject statementsObject = jObject["statements"].ToObject<dynamic>();
            var instanceOfValue = ResolveFirstInstanceValue(statementsObject);

            var properties = ResolveProperties(instanceOfValue, statementsObject);
            WikidataItemBase itemBase = jObject.ToObject<WikidataItemBase>();
            FlatWikidataItemDto flatWikidataItemDto = ResolveBaseData(id, statementsObject, itemBase);
            flatWikidataItemDto.Statements = _helper.GetStatementsValues(statementsObject, properties);

            return flatWikidataItemDto;
        }

        private FlatWikidataItemDto ResolveBaseData(string id, JObject statementsObject, WikidataItemBase itemBase)
        {
            FlatWikidataItemDto flatWikidataItemDto = new FlatWikidataItemDto();

            flatWikidataItemDto.Id = id;
            flatWikidataItemDto.Label = _helper.GetTextValue(itemBase.labels);
            flatWikidataItemDto.Description = _helper.GetTextValue(itemBase.descriptions);
            flatWikidataItemDto.TotalNumberOfStatements = (statementsObject).Count;
            flatWikidataItemDto.Aliases = GetAliases(itemBase.aliases);
            flatWikidataItemDto.UriCollection = GetUriCollection(id, itemBase.sitelinks);
            return flatWikidataItemDto;
        }

        private string ResolveFirstInstanceValue(JObject statementsObject)
        {
            var statements = _helper.GetStatement(statementsObject, Constants.WikidataPropertyIdInstanceOf).Result;
            var instances = statements.Select(x => x.Statement).FirstOrDefault();

            if (instances == null)
                return Constants.Missing;

            return instances[0].value.content.ToString();
        }

        private IEnumerable<string> ResolveProperties(string instanceOfValue, JObject statementsObject)
        {
            // TODO vullen vanuit appsettings, ook Contants  

            switch (instanceOfValue)
            {
                case Constants.WikidataIdHuman:
                    return GetPropertiesOfHuman();
                case Constants.WikidataIdDisambiguationPage:
                    return GetPropertiesOfDisambiguationPage();
                case Constants.WikidataIdAstronomicalObjectType:
                    return GetPropertiesOfAstronomicalObjectType();
                default:
                    // TODO in maxNumberOfCoreDataProperties appsetting
                    int maxNumberOfProperties = 5;
                    return _helper.GetProperties(statementsObject, maxNumberOfProperties);
            }

        }

        private static IEnumerable<string> GetPropertiesOfHuman()
        {

            return new List<string>
            {
                 "P31",   // "instance of" 
                 "P21",   // "sex or gender" 
                 "P27",   // "country of citizenship" 
                 "P735",  // "given name" 
                 "P734",  // "family name" 
                 "P569",  // "date of birth" 
                 "P19",   // "place of birth" 
                 "P570",  // "date of death" 
                 "P20",   // "place of death" 
                 "P106"   // "occupation"
            };
        }

        private static IEnumerable<string> GetPropertiesOfDisambiguationPage()
        {
            // wikimedia disambiguation page = 'Q4167410'
            return new List<string>
            {
                 "P31",   // "instance of" 
                 "P1889", // "different from" 
                 "P1382", // "partially coincident with",
                 "P460"   // "said to be the same as" 
            };
        }

        private static IEnumerable<string> GetPropertiesOfAstronomicalObjectType()
        {
            // astronomical object type = 'Q17444909'
            return new List<string>
            {
                 "P31",  // "instance of" 
                 "P279", // "subclass of" 
                 "P361", // "part of" 
                 "P18",  // "image"
                 "P366", // "has use" 
                 "P367", // "astronomic symbol image" 
                 "P1343" // "described by source" 
            };
        }

        private List<string> GetAliases(Dictionary<string, List<string>> aliases)
        {
            if (!aliases.Any())
                return new List<string> { Constants.Missing };

            if (aliases.Any(a => a.Key == "en"))
                if (aliases["en"].Count > 0)
                    return aliases["en"];

            return aliases.Aggregate((x, y) => x.Value.Count > y.Value.Count ? x : y).Value;
        }

        private UriCollectionDto GetUriCollection(string id, Sitelinks sitelinks)
        {
            return new UriCollectionDto
            {
                WikidataUri = "https://www.wikidata.org/wiki/" + id,
                Wikipedias = GetWikipedias(sitelinks),
            };
        }

        private static List<string> GetWikipedias(Sitelinks sitelinks)
        {
            const int MaximumNumberOfUrisToOutput = 15;

            List<Sitelink?> filledSitelinks = GetFilledSitelinks(sitelinks);

            if (!filledSitelinks.Any())
                return new List<string> { "*no values*" };

            if (filledSitelinks.Count > MaximumNumberOfUrisToOutput)
            {
                var mainSitelinks = CreateMainSitelinks(sitelinks);
                filledSitelinks = GetFilledSitelinks(mainSitelinks);
            }

            return filledSitelinks.Select(w => w.url).ToList();
        }

        private static List<Sitelink?> GetFilledSitelinks(Sitelinks sitelinks)
        {
            return sitelinks.GetType().GetProperties()
                .Where(sl => sl.PropertyType == typeof(Sitelink))
                .Select(sl => (Sitelink?)sl.GetValue(sitelinks))
                .Where(x => x != null).ToList();
        }

        private static Sitelinks CreateMainSitelinks(Sitelinks sitelinks)
        {
            return new Sitelinks
            {
                enwiki = sitelinks.enwiki,
                nlwiki = sitelinks.nlwiki,
                dewiki = sitelinks.dewiki,
                frwiki = sitelinks.frwiki,
                eswiki = sitelinks.eswiki,
                itwiki = sitelinks.itwiki,
                idwiki = sitelinks.idwiki,
                trwiki = sitelinks.trwiki,
                zhwiki = sitelinks.zhwiki,
                ruwiki = sitelinks.ruwiki,
                jawiki = sitelinks.jawiki,
                kowiki = sitelinks.kowiki,
                hiwiki = sitelinks.hiwiki,
                mrwiki = sitelinks.mrwiki,
                tewiki = sitelinks.tewiki,
                arwiki = sitelinks.arwiki
            };
        }
    }
}