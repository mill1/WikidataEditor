using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using WikidataEditor.Dtos;
using WikidataEditor.Interfaces;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class WikidataService : IWikidataService
    {
        private readonly HttpClient _client;

        private const string Missing = "*missing*";

        public WikidataService(HttpClient httpClient)
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "C# Application");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HumanDto GetStatements(string id)
        {
            JObject jsonObject = GetEntityData(id, "statements");

            var statements = jsonObject.ToObject<Statements>();

            var label = GetLabel(id);
            var description = GetDescription(id);

            if (!IsHuman(statements))
            {
                return new HumanDto { Id = id, Label = label, Description = description };
            }
            return MapToDto(id, label, description, statements);
        }

        private HumanDto MapToDto(string id, string label, string description, Statements statements)
        {
            return new HumanDto
            {
                Id = id,
                Label = label,
                Description = description,
                SexOrGender = ResolveValue(statements.P21),
                CountryOfCitizenship = ResolveValue(statements.P27),
                GivenName = ResolveValue(statements.P735),
                FamilyName = ResolveValue(statements.P734),
                DateOfBirth = ResolveTimeValue(statements.P569),
                PlaceOfBirth = ResolveValue(statements.P19),
                DateOfDeath = ResolveTimeValue(statements.P570),
                PlaceOfDeath = ResolveValue(statements.P20),
                Occupation = ResolveValue(statements.P106),
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
            return GetTextValue(id, "labels");
        }

        private string GetDescription(string id)
        {
            return GetTextValue(id, "descriptions");
        }

        private string GetTextValue(string id, string wikidataTypeOfData)
        {
            JObject jsonObject = GetEntityData(id, wikidataTypeOfData);

            var mainCodes = jsonObject.ToObject<MainLanguageCodes>();

            if (mainCodes.en == null)
            {
                var value = GetValueOfFirstFilledProperty(mainCodes);

                if (value != null)
                    return value;

                return jsonObject.Count == 0 ? Missing : ((JValue)((JProperty)jsonObject.First).Value).Value.ToString();
            }
            else
                return mainCodes.en;
        }

        private JObject GetEntityData(string itemId, string wikidataTypeOfData)
        {
            string uri = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + itemId + "/" + wikidataTypeOfData;
            var jsonString = _client.GetStringAsync(uri).Result;

            return JObject.Parse(jsonString);
        }

        private string GetValueOfFirstFilledProperty(MainLanguageCodes codes)
        {
            // Q114658910
            return codes.GetType().GetProperties()
            .Where(pi => pi.PropertyType == typeof(string))
            .Select(pi => (string)pi.GetValue(codes))
            .FirstOrDefault(value => !string.IsNullOrEmpty(value));
        }

        private static bool IsHuman(Statements statements)
        {
            const string Human = "Q5";

            if (statements.P31 == null)
                return false;

            return statements.P31.Any(prop => prop.value.content.ToString() == Human);
        }
    }
}