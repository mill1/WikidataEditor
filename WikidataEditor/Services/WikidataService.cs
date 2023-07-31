using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using WikidataEditor.Dtos;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class WikidataService 
    {
        private readonly HttpClient _client;

        public WikidataService(HttpClient httpClient)
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "C# Application");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public WikidataStatementsDto GetStatements(string id)
        {
            string uri = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + id + @"/statements";

            var jsonString = _client.GetStringAsync(uri).Result;

            JObject jsonObject = JObject.Parse(jsonString);

            if (jsonObject == null)
                throw new ArgumentException($"Item {id} could not be deserialized");

            var statements = jsonObject.ToObject<Statements>();

            if (!IsHuman(statements))
            {
                return new WikidataStatementsDto
                {
                    Id = id,
                    IsHuman = false
                };
            }
            return MapToDto(id, statements);
        }

        private IEnumerable<string> ResolveValue(Statement[] statement)
        {
            // A 'statement' can consist of multiple values (claims about the statement)
            if(statement == null)
                return new List<string> { "*missing*" };
            
            return statement.Select(x => GetLabel(x.value.content.ToString()));
        }

        private string GetTimeValue(object content)
        {
            var timeProperty = ((JContainer)content).Where(p => ((JProperty)p).Name == "time").FirstOrDefault();

            if (timeProperty == null)
                return null;
            
            return ((JValue)((JProperty)timeProperty).Value).Value.ToString();
        }

        private string GetLabel(string wikidataItemId)
        {
            var uri = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + wikidataItemId + @"/labels";

            var jsonString = _client.GetStringAsync(uri).Result;
            JObject labels = JObject.Parse(jsonString);
            var label = labels.ToObject<LabelEnglish>();

            if (label.en == null)
                return labels.Count == 0 ? null : ((JValue)((JProperty)labels.First).Value).Value.ToString();
            else
                return label.en;
        }

        private static bool IsHuman(Statements statements)
        {
            return statements.P31.Any(prop => prop.value.content.ToString() == "Q5");
        }

        private WikidataStatementsDto MapToDto(string id, Statements statements)
        {
            return new WikidataStatementsDto
            {
                Id = id,
                IsHuman = IsHuman(statements),
                SexOrGender = ResolveValue(statements.P21),
                CountryOfCitizenship = ResolveValue(statements.P27),
                GivenName = ResolveValue(statements.P735),
                FamilyName = ResolveValue(statements.P734),
                DateOfBirth = statements.P569.Select(x => GetTimeValue(x.value.content)),
                PlaceOfBirth = ResolveValue(statements.P19),
                DateOfDeath = statements.P570.Select(x => GetTimeValue(x.value.content)),
                PlaceOfDeath = ResolveValue(statements.P20),
                Occupation = ResolveValue(statements.P106),
            };
        }
    }
}