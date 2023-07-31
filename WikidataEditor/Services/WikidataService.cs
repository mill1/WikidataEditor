using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Xml.Linq;
using WikidataEditor.Dtos;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class WikidataService 
    {
        private readonly HttpClient _client;

        public WikidataService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "C# Application");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public WikidataStatementsDto GetStatements(string id)
        {
            string uri = @"https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + id + @"/statements";

            var jsonString = _client.GetStringAsync(uri).Result;

            JObject statements = JObject.Parse(jsonString);

            if (statements == null)
                throw new ArgumentException($"Item {id} could not be deserialized");

            // TODO obviously:
            // Casten naar meest uitgebreide: StatementsQ8016.cs moet uiteindelijk worden: Statements.cs
            // Uitzonderingen afvangen in controller?
            var statementsCunliffe = statements.ToObject<StatementsQ99589194>();

            if (!IsHuman(statementsCunliffe))
            {
                return new WikidataStatementsDto
                {
                    Id = id,
                    IsHuman = false
                };
            }

            return new WikidataStatementsDto
            {
                Id = id,
                IsHuman = IsHuman(statementsCunliffe),
                SexOrGender = ResolveValue(statementsCunliffe.P21),
                CountryOfCitizenship = ResolveValue(statementsCunliffe.P27),
                GivenName = ResolveValue(statementsCunliffe.P735),
                FamilyName = ResolveValue(statementsCunliffe.P734),
                DateOfBirth = statementsCunliffe.P569.Select(x => GetTimeValue(x.value.content)),
                PlaceOfBirth = ResolveValue(statementsCunliffe.P19),
                DateOfDeath = statementsCunliffe.P570.Select(x => GetTimeValue(x.value.content)),
                PlaceOfDeath = ResolveValue(statementsCunliffe.P20),
                // TODO: DTO containing most important human wikidata statements;
                // aantal dto-props moet dus overeenkomen met aantal classes in StatementTypes.cs
            };
        }

        private IEnumerable<string> ResolveValue(Statement[] statement) //StatementsQ99589194 statementsCunliffe)
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

        private static bool IsHuman(StatementsQ99589194 statementsQ99589194)
        {
            return statementsQ99589194.P31.Any(prop => prop.value.content.ToString() == "Q5");
        }
    }
}