using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using WikidataEditor.Dtos;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class WikidataRestService : IWikidataRestService
    {
        private readonly HttpClient _client;

        private const string Missing = "*missing*";

        public WikidataRestService(HttpClient httpClient)
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "C# Application");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IWikidataItemDto GetDataOnHuman(string id)
        {
            IWikidataItem item = null;
            WikidataItemBaseDto basicData = null;
            string Uri = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + id;
            var jsonString = _client.GetStringAsync(Uri).Result;

            var jObject = JObject.Parse(jsonString);

            var type = (string)((JValue)jObject["type"])?.Value ?? "null";
            if (type != "item")
                throw new ArgumentException($"Result is not of type item. Encountered type: {type}");

            var statements = jObject["statements"].ToObject<dynamic>();
            var statementInstanceOf = statements["P31"];
            int statementsCount = ((JContainer)statements).Count;

            if (statementInstanceOf == null)
            {
                item = jObject.ToObject<WikidataItemOther>();               
                basicData = ResolveBasicData(item, null, statementsCount);
                return new WikidataItemOtherDto(basicData);
            }
            
            statementInstanceOf = statementInstanceOf.ToObject<Statement[]>();

            switch (statementInstanceOf)
            {
                default:
                    break;
            }

            // return null;

            //if (!IsHuman(basicData.InstanceOf))
            //    return new WikidataItemHumanDto(basicData);

            return MapToHumanDto(basicData, (WikidataItemOnHumans)item);
        }

        private WikidataItemBaseDto ResolveBasicData(IWikidataItem item, Statement[] statementInstanceOf, int statementsCount)
        {
            return new WikidataItemBaseDto
            {
                Id = item.id,
                Label = GetTextValue(item.labels),
                InstanceOf = ResolveInstanceTexts(statementInstanceOf),
                Description = GetTextValue(item.descriptions),
                StatementsCount = statementsCount,
                Aliases = GetAliases(item.aliases),
                UriCollection = GetUriCollection(item)
            };
        }

        private IEnumerable<string> ResolveInstanceTexts(Statement[] instances)
        {
            var values = ResolveValue(instances);

            if (values.First() == Missing)
                return values;

            var ids = instances.Select(i => i.value.content.ToString());
            return values.Zip(ids, (first, second) => first + " (" + second + ")");
        }

        private WikidataItemHumanDto MapToHumanDto(WikidataItemBaseDto basicData, WikidataItemOnHumans item)
        {
            return new WikidataItemHumanDto(basicData)
            {
                SexOrGender = ResolveValue(item.statements.P21),
                CountryOfCitizenship = ResolveValue(item.statements.P27),
                GivenName = ResolveValue(item.statements.P735),
                FamilyName = ResolveValue(item.statements.P734),
                DateOfBirth = ResolveTimeValue(item.statements.P569),
                PlaceOfBirth = ResolveValue(item.statements.P19),
                DateOfDeath = ResolveTimeValue(item.statements.P570),
                PlaceOfDeath = ResolveValue(item.statements.P20),
                Occupation = ResolveValue(item.statements.P106),
            };
        }

        private IEnumerable<string> ResolveValue(Statement[] statement)
        {
            // A 'statement' can consist of multiple values (claims about the statement)
            if (statement == null)
                return new List<string> { Missing };

            return statement.Select(x => x.value.content == null ? "*no value*" : GetLabel(x.value.content.ToString()));
        }

        private IEnumerable<string> ResolveTimeValue(Statement[] statement)
        {
            if (statement == null)
                return new List<string> { Missing };

            return statement.Select(x => GetTimeValue(x.value.content));
        }

        private string GetTimeValue(object content)
        {
            var timeProperty = ((JContainer)content).Where(p => ((JProperty)p).Name == "time").FirstOrDefault();

            if (timeProperty == null)
                return Missing;

            return ((JValue)((JProperty)timeProperty).Value).Value.ToString();
        }

        private string GetLabel(string id)
        {
            JObject jsonObject = GetEntityData(id, "labels");

            var codes = jsonObject.ToObject<LanguageCodes>();

            if (codes.en != null)
                return codes.en;

            var value = GetValueOfFirstFilledProperty(codes);

            if (value != null)
                return value;

            return jsonObject.Count == 0 ? Missing : ((JValue)((JProperty)jsonObject.First).Value).Value.ToString();
        }

        private JObject GetEntityData(string itemId, string wikidataTypeOfData)
        {
            string Uri = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + itemId + "/" + wikidataTypeOfData;
            var jsonString = _client.GetStringAsync(Uri).Result;

            return JObject.Parse(jsonString);
        }

        private List<string> GetAliases(Dictionary<string, List<string>> aliases)
        {
            if (!aliases.Any())
                return new List<string> { Missing };

            if (aliases.Any(a => a.Key == "en"))
                if (aliases["en"].Count > 0)
                    return aliases["en"];

            return aliases.Aggregate((x, y) => x.Value.Count > y.Value.Count ? x : y).Value;
        }

        private UriCollectionDto GetUriCollection(IWikidataItem item)
        {
            return new UriCollectionDto
            {
                WikidataUri = "https://www.wikidata.org/wiki/" + item.id,
                Wikipedias = GetWikipedias(item.sitelinks),
                // TODO specifiek voor humans in generieke functie?
                //InstanceUris = ResolveInstanceUrisForHumans((WikidataItemOnHumans)item)
            };
        }

        private List<string> ResolveInstanceUrisForHumans(WikidataItemOnHumans item)
        {
            const string uriBaseLoCAuthority = "https://id.loc.gov/authorities/names/";
            const string uriBaseBNF = "https://data.bnf.fr/en/";

            var instanceUris = new List<string>();

            if (item.statements.P244 != null)
                instanceUris.Add(uriBaseLoCAuthority + item.statements.P244.First().value.content + ".html");

            if (item.statements.P268 != null)
                instanceUris.Add(uriBaseBNF + item.statements.P268.First().value.content.ToString().Substring(0, 8));

            if (!instanceUris.Any())
                instanceUris.Add("*no values*");

            return instanceUris;
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

        private string GetTextValue(LanguageCodes codes)
        {
            if (codes.en == null)
            {
                var value = GetValueOfFirstFilledProperty(codes);

                if (value == null)
                    return Missing;

                return value;
            }

            return codes.en;
        }

        private string GetValueOfFirstFilledProperty(LanguageCodes codes)
        {
            // Q114658910
            return codes.GetType().GetProperties()
            .Where(c => c.PropertyType == typeof(string))
            .Select(c => (string)c.GetValue(codes))
            .FirstOrDefault(value => !string.IsNullOrEmpty(value));
        }

        private static bool IsHuman(IEnumerable<string> Instances)
        {
            if (Instances.First() == Missing)
                return false;

            return Instances.Any(instance => instance == "human (Q5)");
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