using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using WikidataEditor.Dtos;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class WikidataService 
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

        public WikidataStatementsDto GetStatements(string id)
        {
            string uri = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + id + @"/statements";

            var jsonString = _client.GetStringAsync(uri).Result;

            JObject jsonObject = JObject.Parse(jsonString);

            var statements = jsonObject.ToObject<Statements>();

            if (!IsHuman(statements))
            {
                return new WikidataStatementsDto{ Id = id, IsHuman = false };
            }
            return MapToDto(id, statements);
        }

        private WikidataStatementsDto MapToDto(string id, Statements statements)
        {
            return new WikidataStatementsDto
            {
                Id = id,
                Label = GetLabel(id),
                IsHuman = IsHuman(statements),
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
            if(statement == null)
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
                return null;
            
            return ((JValue)((JProperty)timeProperty).Value).Value.ToString();
        }

        private string GetLabel(string id)
        {
            var uri = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + id + @"/labels";

            var jsonString = _client.GetStringAsync(uri).Result;
            JObject labels = JObject.Parse(jsonString);

            var label = labels.ToObject<LabelEnglish>();

            if (label.en == null)
                return labels.Count == 0 ? Missing : ((JValue)((JProperty)labels.First).Value).Value.ToString();
            else
                return label.en;
        }

        private static bool IsHuman(Statements statements)
        {
            if(statements.P31 == null)
                return false;

            return statements.P31.Any(prop => prop.value.content.ToString() == "Q5");
        }
    }
}