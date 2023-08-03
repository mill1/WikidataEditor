using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;
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

        public HumanDto GetDataOnHuman(string id)
        {
            string uri = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + id;
            var jsonString = _client.GetStringAsync(uri).Result;

            var jObject = JObject.Parse(jsonString);
            //var labels = jObject["labels"].ToObject<LanguageCodes>(); also works
            var item = jObject.ToObject<WikidataItem>();
           
            var label = GetTextValue(item.labels);
            var description = GetTextValue(item.descriptions);

            if (!IsHuman(item.statements.P31))
            {
                return new HumanDto { Id = id, Label = label, Description = description };
            }
            return MapToDto(id, label, description, item);
        }

        private HumanDto MapToDto(string id, string label, string description, WikidataItem item)
        {
            return new HumanDto
            {
                Id = id,
                Label = label,
                Description = description,
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
                LibraryOfCongressAuthorityURI = GetLibraryOfCongressAuthorityURI(item.statements.P244)
            };
        }

        private IEnumerable<string> ResolveValue(Statement[] statement)
        {
            // A 'statement' can consist of multiple values (claims about the statement)
            if (statement == null)
                return new List<string> { Missing };

            return statement.Select(x => GetLabel(x.value.content.ToString()));
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

        private string GetLibraryOfCongressAuthorityURI(Statement[] statement)
        {
            const string uriBase = "https://id.loc.gov/authorities/names/";

            if (statement == null)
                return Missing;

            return uriBase + statement.First().value.content + ".html";
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
            .Where(pi => pi.PropertyType == typeof(string))
            .Select(pi => (string)pi.GetValue(codes))
            .FirstOrDefault(value => !string.IsNullOrEmpty(value));
        }

        private static bool IsHuman(Statement[] statementsOnInstance)
        {
            const string Human = "Q5";

            if (statementsOnInstance == null)
                return false;

            return statementsOnInstance.Any(prop => prop.value.content.ToString() == Human);
        }
    }
}