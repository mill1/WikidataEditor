﻿using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class WikidataRestService : IWikidataRestService
    {
        private class Parameters
        {
            public Parameters(string id, string label, string description, int statementsCount)
            {
                Id = id;
                Label = label;
                Description = description;
                StatementsCount = statementsCount;
            }

            public string Id { get; }
            public string Label { get; }
            public string Description { get; }
            public int StatementsCount { get; }
        }

        private readonly HttpClient _client;

        private const string Missing = "*missing*";

        public WikidataRestService(HttpClient httpClient)
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "C# Application");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HumanDto GetDataOnHuman(string id)
        {
            string uri = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + id;
            var jsonString = _client.GetStringAsync(uri).Result;

            var jObject = JObject.Parse(jsonString);                  
            var item = jObject.ToObject<WikidataItem>();

            if (item.type != "item")
                throw new ArgumentException($"Response is not of type item. Encountered type: {item.type}");

            var statements = jObject["statements"].ToObject<dynamic>();
            Parameters parameters = ResolveBasicParameters(item, ((JContainer)statements).Count);
            
            if (!IsHuman(item.statements.P31))
                return CreateNonHumanDto(parameters, item);

            return MapToDto(parameters, item);
        }

        private HumanDto CreateNonHumanDto(Parameters parameters, WikidataItem item)
        {
            return new HumanDto 
            { 
                Id = parameters.Id, 
                Label = parameters.Label, 
                Description = parameters.Description, 
                StatementsCount = parameters.StatementsCount,
                Aliases = GetAliases(item.aliases),
                UriCollection = GetUriCollection(item)
            };
        }

        private Parameters ResolveBasicParameters(WikidataItem item, int statementsCount)
        {
            var label = GetTextValue(item.labels);
            var description = GetTextValue(item.descriptions);

            return new Parameters(item.id, label, description, statementsCount);
        }

        private HumanDto MapToDto(Parameters parameters, WikidataItem item)
        {
            return new HumanDto
            {
                Id = parameters.Id,
                Label = parameters.Label,
                Description = parameters.Description,  
                StatementsCount = parameters.StatementsCount,
                Aliases = GetAliases(item.aliases),
                SexOrGender = ResolveValue(item.statements.P21),
                CountryOfCitizenship = ResolveValue(item.statements.P27),
                GivenName = ResolveValue(item.statements.P735),
                FamilyName = ResolveValue(item.statements.P734),
                DateOfBirth = ResolveTimeValue(item.statements.P569),
                PlaceOfBirth = ResolveValue(item.statements.P19),
                DateOfDeath = ResolveTimeValue(item.statements.P570),
                PlaceOfDeath = ResolveValue(item.statements.P20),
                Occupation = ResolveValue(item.statements.P106),
                UriCollection = GetUriCollection(item)
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
            string uri = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + itemId + "/" + wikidataTypeOfData;
            var jsonString = _client.GetStringAsync(uri).Result;

            return JObject.Parse(jsonString);
        }

        private List<string> GetAliases(Dictionary<string, List<string>> aliases)
        {
            if (!aliases.Any())
                return new List<string> { Missing };

            if(aliases.Any(a => a.Key == "en"))
                if (aliases["en"].Count > 0)
                    return aliases["en"]; 

            return aliases.Aggregate((x, y) => x.Value.Count > y.Value.Count ? x : y).Value;
        }

        private URICollectionDto GetUriCollection(WikidataItem item)
        {            
            return new URICollectionDto
            {
                WikidataURI = "https://www.wikidata.org/wiki/" + item.id,
                LibraryOfCongressAuthorityURI = GetLibraryOfCongressAuthorityURI(item.statements.P244),
                Wikipedias = GetWikipedias(item.sitelinks)
            };
        }

        private static List<string> GetWikipedias(Sitelinks sitelinks)
        {
            const int MaximumNumberOfUrisToOutput = 15;

            List<Sitelink?> filledSitelinks = GetFilledSitelinks(sitelinks);

            if (!filledSitelinks.Any())
                return new List<string> { Missing };

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

        private string GetLibraryOfCongressAuthorityURI(Statement[] statement)
        {
            if (statement == null)
                return Missing;

            return "https://id.loc.gov/authorities/names/" + statement.First().value.content + ".html";
        }

        private string GetTextValue(LanguageCodes codes)
        {
            if (codes.en == null)
            {
                var value = GetValueOfFirstFilledProperty(codes);

                if(value == null) 
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

        private static bool IsHuman(Statement[] statementsOnInstance)
        {
            const string Human = "Q5";

            if (statementsOnInstance == null)
                return false;

            return statementsOnInstance.Any(prop => prop.value.content.ToString() == Human);
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