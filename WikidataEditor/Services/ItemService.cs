using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using WikidataEditor.Common;
using WikidataEditor.Configuration;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.CoreData;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class ItemService : IItemService
    {
        private readonly IHttpClientWikidataApi _clientWikipediaApi;
        private readonly IWikidataHelper _helper;
        private readonly CoreDataOptions? _coreDataOptions;

        public ItemService(IConfiguration configuration, IHttpClientWikidataApi httpClientWikidataApi, IWikidataHelper wikidataHelper)
        {
            _clientWikipediaApi = httpClientWikidataApi;
            _helper = wikidataHelper;

            _coreDataOptions = new CoreDataOptions();
            configuration.GetSection(CoreDataOptions.CoreData).Bind(_coreDataOptions);
        }

        public WikidataItemDto Get(string id)
        {
            var jsonString = _clientWikipediaApi.GetStringAsync("items/" + id).Result;

            JObject jObject = JObject.Parse(jsonString);

            var item = jObject.ToObject<WikidataItemBase>();

            var itemDto = new WikidataItemDto(item.id, item.type, item.aliases);

            itemDto.Labels = GetFilledTexts(item.labels);
            itemDto.Descriptions = GetFilledTexts(item.descriptions);            
            itemDto.Sitelinks = GetFilledSitelinks(item.sitelinks); 

            return itemDto;
        }

        private IEnumerable<EntityTextDto> GetFilledTexts(LanguageCodes codes)
        {
            return codes.GetType().GetProperties()
                .Where(c => c.PropertyType == typeof(string))
                .Select(c => new EntityTextDto
                {
                    LanguageCode = c.Name,
                    Value = (string)c.GetValue(codes)
                })
                .Where(text => !string.IsNullOrEmpty((string)text.Value));
        }

        public FlatWikidataItemDto GetCoreData(string id)
        {
            var jsonString = _clientWikipediaApi.GetStringAsync("items/" + id).Result;

            JObject jObject = JObject.Parse(jsonString);
            JObject statementsObject = jObject["statements"].ToObject<dynamic>();
            var instanceOfValue = ResolveFirstInstanceValue(statementsObject);

            var properties = ResolveProperties(instanceOfValue, statementsObject);
            var itemBase = jObject.ToObject<WikidataItemBase>();
            var flatWikidataItemDto = ResolveBaseData(id, statementsObject, itemBase);
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
            var configuredItem = _coreDataOptions.WikidataItems.Where(item => item.WikidataItemId == instanceOfValue).FirstOrDefault();

            if (configuredItem != null)
                return configuredItem.Properties;

            return _helper.GetProperties(statementsObject, _coreDataOptions.MaxNumberOfProperties);
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