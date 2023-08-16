using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;

namespace WikidataEditor.Common
{
    public class WikidataHelper : IWikidataHelper
    {
        private static readonly Regex WikidataIdPattern = new(@"^Q\d{1}", RegexOptions.Compiled);
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;

        public WikidataHelper(IHttpClientWikidataApi httpClientWikidataApi)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
        }

        public async Task<IEnumerable<StatementsDto>> GetStatements(string id)
        {
            JObject jsonObject = await GetEntityData(id, "statements");
            var statementsObject = jsonObject.ToObject<dynamic>();

            var statements = new List<StatementsDto>();

            foreach (var statementObject in statementsObject)
            {
                var property = ((JProperty)statementObject).Name;
                var jArray = ((JProperty)statementObject).Value;
                var array = jArray.ToObject<Statement[]>();

                statements.Add(
                    new StatementsDto
                    {
                        Property = property,
                        Statement = array
                    }
                );
            }
            return statements;
        }

        public IEnumerable<string> GetProperties(dynamic statementsObject, int maxNumberOfProperties)
        {
            var properties = new List<string>();
            int count = 0;

            foreach (var statementObject in statementsObject)
            {
                string property = ((JProperty)statementObject).Name;
                properties.Add(property);
                count++;

                if (count >= maxNumberOfProperties)
                    return properties;
            }
            return properties;
        }

        public async Task<IEnumerable<StatementsDto>> GetStatement(string id, string property)
        {
            JObject jsonObject = await GetEntityData(id, "statements");
            var statementsObject = jsonObject.ToObject<dynamic>();

            return await GetStatement(statementsObject, property);
        }

        public async Task<IEnumerable<StatementsDto>> GetStatement(JObject statementsObject, string property)
        {
            var statement = statementsObject[property];

            if (statement == null)
                return CreateStatement(property, $"Property {property} not found in statements");

            return new List<StatementsDto>
            {
                new StatementsDto
                {
                    Property = property,
                    Statement = statement.ToObject<Statement[]>()
                }
            };
        }

        public List<FlatStatementDto> GetStatementsValues(dynamic statementsObject, IEnumerable<string> properties)
        {
            var flatStatements = new List<FlatStatementDto>();

            foreach (var property in properties)
            {
                var statements = (IEnumerable<StatementsDto>)GetStatement(statementsObject, property).Result;
                var statement = statements.Select(x => x.Statement).First();

                flatStatements.Add(
                    new FlatStatementDto
                    {
                        Property = ResolvePropertyLabel(property),
                        Values = ResolveValues(statement)
                    }
                );
            }
            return flatStatements;
        }

        private static string ResolvePropertyLabel(string property)
        {
            string description;

            if (WikidataProperties.Labels.TryGetValue(property, out description))
            {
                return $"{description} ({property})";
            }

            return $"https://www.wikidata.org/wiki/Property:{property}"; ;
        }

        private static List<StatementsDto> CreateStatement(string property, string content)
        {
            return new List<StatementsDto>
                {
                   new StatementsDto
                   {
                       Property = property,
                       Statement = new List<Statement>
                       { new Statement
                       {
                           value = new Value
                           {
                               content = content
                           }
                       }
                       }.ToArray()
                    }
                };
        }

        public async Task<IEnumerable<EntityTextDto>> GetAliases(string id)
        {
            JObject jsonObject = await GetEntityData(id, "aliases");
            var aliasesDictionary = jsonObject.ToObject<Dictionary<string, List<string>>>();

            return aliasesDictionary.Select(a =>
                new EntityTextDto
                {
                    LanguageCode = a.Key,
                    Value = a.Value,
                }
            );
        }

        public async Task<IEnumerable<EntityTextDto>> GetEntityTexts(string id, string entityType)
        {
            var entityTexts = new List<EntityTextDto>();

            JObject jsonObject = await GetEntityData(id, entityType);

            foreach (var lc in jsonObject.ToObject<dynamic>())
            {
                entityTexts.Add(new EntityTextDto
                {
                    LanguageCode = ((JProperty)lc).Name,
                    Value = (string)((JProperty)lc).Value
                });
            }
            return entityTexts;
        }

        public async Task<IEnumerable<EntityTextDto>> GetEntityText(string id, string languageCode, string entityType)
        {
            string uri = "items/" + id + "/" + entityType + "/" + languageCode;
            var jsonString = await _httpClientWikidataApi.GetStringAsync(uri);

            object value = entityType == "aliases" ? JsonConvert.DeserializeObject<List<string>>(jsonString) : JsonConvert.DeserializeObject<string>(jsonString);

            return new List<EntityTextDto>
            {
                new EntityTextDto
                {
                    LanguageCode = languageCode,
                    Value = value
                }
            };
        }

        public IEnumerable<string> ResolveValues(Statement[] statements)
        {
            // A 'statement' can consist of multiple statement values (claims about the statement)
            if (statements == null)
                return new List<string> { Constants.Missing };

            var values = new List<string>();

            foreach (var statement in statements)
            {
                if (statement.value == null)
                {
                    values.Add("*no value*");
                    continue;
                }
                ResolveValue(values, statement);
            }
            return values;
        }

        private void ResolveValue(List<string> values, Statement statement)
        {
            var value = statement.value.content.ToString();
            Match match = WikidataIdPattern.Match(value);

            if (match.Success)
            {
                values.Add(GetLabel(value));
            }
            else
            {
                TimeContent timeContent = TryGetTimeContent(value);

                if (timeContent != null)
                {
                    values.Add(timeContent.time);
                    return;
                }

                GlobeCoordinateContent gcContent = TryGetGlobeCoordinateContent(value);

                if (gcContent != null)
                {
                    string altitude = gcContent.altitude == 0 ? "" : $" Altitude: {gcContent.altitude}";

                    values.Add($"Latitude: {gcContent.latitude} Longitude: {gcContent.longitude}{altitude}");
                    return;
                }

                values.Add(value);
            }
        }

        private TimeContent TryGetTimeContent(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<TimeContent>(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private GlobeCoordinateContent TryGetGlobeCoordinateContent(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<GlobeCoordinateContent>(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetLabel(string id)
        {
            JObject jsonObject = GetEntityData(id, "labels").Result;

            var codes = jsonObject.ToObject<LanguageCodes>();

            if (codes.en != null)
                return codes.en;

            var value = GetValueOfFirstFilledProperty(codes);

            if (value != null)
                return value;

            return Constants.Missing;
        }

        public string GetSingleTextValue(LanguageCodes codes)
        {
            if (codes.en == null)
            {
                var value = GetValueOfFirstFilledProperty(codes);

                if (value == null)
                    return Constants.Missing;

                return value;
            }
            return codes.en;
        }

        private async Task<JObject> GetEntityData(string id, string entityType)
        {
            string uri = "items/" + id + "/" + entityType;
            var jsonString = await _httpClientWikidataApi.GetStringAsync(uri);
            return JObject.Parse(jsonString);
        }

        private string GetValueOfFirstFilledProperty(LanguageCodes codes)
        {
            return codes.GetType().GetProperties()
            .Where(c => c.PropertyType == typeof(string))
            .Select(c => (string)c.GetValue(codes))
            .FirstOrDefault(value => !string.IsNullOrEmpty(value));
        }
    }
}
