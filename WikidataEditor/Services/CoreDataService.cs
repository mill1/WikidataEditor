﻿using Newtonsoft.Json.Linq;
using System.Reflection;
using WikidataEditor.Common;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.CoreData;
using WikidataEditor.Models;
using WikidataEditor.Models.Instances;

namespace WikidataEditor.Services
{
    public class CoreDataService : ICoreDataService
    {
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;
        private readonly IMappingService _mappingService;
        private readonly IWikidataHelper _helper;

        public CoreDataService(IHttpClientWikidataApi httpClientWikidataApi, IMappingService mappingService, IWikidataHelper wikidataHelper)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
            _mappingService = mappingService;
            _helper = wikidataHelper;
        }

        public IWikidataItemDto Get(string id)
        {
            var jsonString = _httpClientWikidataApi.GetStringAsync("items/" + id).Result;

            JObject jObject = JObject.Parse(jsonString);

            var type = (string)((JValue)jObject["type"])?.Value ?? "null";
            if (type != "item")
                throw new ArgumentException($"Result is not of type item. Encountered type: {type}");

            var statementsObject = jObject["statements"].ToObject<dynamic>();
            var statementInstanceOf = statementsObject["P31"];
            int statementsCount = ((JContainer)statementsObject).Count;

            if (statementInstanceOf == null)
            {
                WikidataItemOther item = jObject.ToObject<WikidataItemOther>();
                return new WikidataItemOtherDto(ResolveBasicData(item, null, statementsCount, null));
            }

            // Other types of items
            WikidataItemOther itemOther = jObject.ToObject<WikidataItemOther>();
            itemOther.FlatStatements = GetStatementsValues(statementsObject);

            var y = ResolveBasicData(itemOther, statementInstanceOf, statementsCount, null);
            var x = new WikidataItemOtherDto(y);

            return x;


            //return ResolveData(statementInstanceOf.ToObject<Statement[]>(), jObject, statementsCount);
        }


        public List<FlatStatementDto> GetStatementsValues(dynamic statementsObject)
        {
            var statements = new List<FlatStatementDto>();

            foreach (var statementObject in statementsObject)
            {
                var property = ((JProperty)statementObject).Name;
                var jArray = ((JProperty)statementObject).Value;
                var array = jArray.ToObject<Statement[]>();

                statements.Add(
                    new FlatStatementDto
                    {
                        Property = property, // _helper.GetLabel(property),
                        Statement = _helper.ResolveValues(array)
                    }
                );
            }

            return statements;
        }

        // TODO: generalize further without using Reflection
        private IWikidataItemDto ResolveData(Statement[] statementInstanceOf, JObject jObject, int statementsCount)
        {
            if (ContainsValue(statementInstanceOf, Constants.WikidataIdHuman))
            {
                WikidataItemOnHumans item = jObject.ToObject<WikidataItemOnHumans>();
                WikidataItemBaseDto basicData = ResolveBasicData(item, statementInstanceOf, statementsCount, ResolveUrisForHumans(item));
                return _mappingService.MapToHumanDto(basicData, item);
            }

            if (ContainsValue(statementInstanceOf, Constants.WikidataIdDisambiguationPage))
            {
                WikidataItemOnDisambiguationPages item = jObject.ToObject<WikidataItemOnDisambiguationPages>();
                WikidataItemBaseDto basicData = ResolveBasicData(item, statementInstanceOf, statementsCount, null);
                return _mappingService.MapToDisambiguationPageDto(basicData, item);
            }

            if (ContainsValue(statementInstanceOf, Constants.WikidataIdAstronomicalObjectType))
            {
                WikidataItemOnAstronomicalObjectTypes item = jObject.ToObject<WikidataItemOnAstronomicalObjectTypes>();
                WikidataItemBaseDto basicData = ResolveBasicData(item, statementInstanceOf, statementsCount, ResolveUrisForAstronomicalObjectTypes(item));
                return _mappingService.MapToAstronomicalObjectTypeDto(basicData, item);
            }

            // Other types of items
            WikidataItemOther itemOther = jObject.ToObject<WikidataItemOther>();
            return new WikidataItemOtherDto(ResolveBasicData(itemOther, statementInstanceOf, statementsCount, null));
        }

        private WikidataItemBaseDto ResolveBasicData(IWikidataItem item, Statement[] statementInstanceOf, int statementsCount, List<string> instanceUris)
        {
            return new WikidataItemBaseDto
            {
                Id = item.id,
                Label = _helper.GetTextValue(item.labels),
                InstanceOf = ResolveInstanceTexts(statementInstanceOf),
                Description = _helper.GetTextValue(item.descriptions),
                StatementsCount = statementsCount,
                Aliases = GetAliases(item.aliases),
                UriCollection = GetUriCollection(item, instanceUris)
            };
        }

        private IEnumerable<string> ResolveInstanceTexts(Statement[] instances)
        {
            var values = _helper.ResolveValues(instances);

            if (values.First() == Constants.Missing)
                return values;

            var ids = instances.Select(id => id.value.content.ToString());
            return values.Zip(ids, (first, second) => first + " (" + second + ")");
        }

        private static bool ContainsValue(Statement[] statements, string value)
        {
            if (statements == null)
                return false;

            return statements.Any(s => s.value.content.ToString() == value);
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

        private UriCollectionDto GetUriCollection(IWikidataItem item, List<string> instanceUris)
        {
            return new UriCollectionDto
            {
                WikidataUri = "https://www.wikidata.org/wiki/" + item.id,
                Wikipedias = GetWikipedias(item.sitelinks),
                InstanceUris = instanceUris
            };
        }

        private List<string> ResolveUrisForHumans(WikidataItemOnHumans item)
        {
            var instanceUris = new List<string>();

            const string uriBaseLoCAuthority = "https://id.loc.gov/authorities/names/";

            ResolveUriForBNF(item.statements, instanceUris);

            if (item.statements.P244 != null)
                instanceUris.Add(uriBaseLoCAuthority + item.statements.P244.First().value.content + ".html");

            if (!instanceUris.Any())
                instanceUris.Add("*no values*");

            return instanceUris;
        }

        private List<string> ResolveUrisForAstronomicalObjectTypes(WikidataItemOnAstronomicalObjectTypes item)
        {
            var instanceUris = new List<string>();

            ResolveUriForBNF(item.statements, instanceUris);

            if (!instanceUris.Any())
                instanceUris.Add("*no values*");

            return instanceUris;
        }

        private static void ResolveUriForBNF(IUriStatements uriStatements, List<string> instanceUris)
        {
            const string uriBaseBNF = "https://data.bnf.fr/en/";

            if (uriStatements.P268 != null)
                instanceUris.Add(uriBaseBNF + uriStatements.P268.First().value.content.ToString().Substring(0, 8));
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