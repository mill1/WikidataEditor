using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;
using WikidataEditor.Models.Instances;

namespace WikidataEditor.Common
{
    public class WikidataHelper : IWikidataHelper
    {
        private static readonly Regex WikidataIdPattern = new(@"Q\d{1}", RegexOptions.Compiled);
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;

        public WikidataHelper(IHttpClientWikidataApi httpClientWikidataApi)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
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

        public async Task<IEnumerable<StatementsDto>> GetStatement(string id, string property)
        {
            JObject jsonObject = await GetEntityData(id, "statements");
            var statementsObject = jsonObject.ToObject<dynamic>();

            foreach (var statementObject in statementsObject)
            {
                string propertyName = ((JProperty)statementObject).Name;

                if (propertyName== property)
                {
                    var jArray = ((JProperty)statementObject).Value;

                    return new List<StatementsDto>
                    {
                        new StatementsDto
                        {
                            Property = property,
                            Statement = jArray.ToObject<Statement[]>()
                        }
                    };
                }
            }

            throw new HttpRequestException($"Property {property} not found in statements on {id}{ResolveOnLabel(id)}", null, System.Net.HttpStatusCode.NotFound);
        }

        private string ResolveOnLabel(string id)
        {
            string label;
            try
            {
                label = GetLabel(id);
            }
            catch (Exception)
            {
                label = null;
            }
            return label == null ? string.Empty : $" ({label})";
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

        public async Task<IEnumerable<EntityTextDto>> GetAliases(string id)
        {
            JObject jsonObject = await GetEntityData(id, "aliases");
            var aliasesDictionary = jsonObject.ToObject<Dictionary<string, List<string>>>();

            return aliasesDictionary.Select( a =>
                new EntityTextDto
                {
                    LanguageCode = a.Key,
                    Value = a.Value,
                }
            );
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

        public IEnumerable<string> ResolveValue(Statement[] statements)
        {
            // A 'statement' can consist of multiple statement values (claims about the statement)
            if (statements == null)
                return new List<string> { Constants.Missing };

            var labelValues = new List<string>();

            foreach (var statement in statements)
            {
                if (statement.value == null)
                    labelValues.Add("*no value*");

                var value = statement.value.content.ToString();
                Match match = WikidataIdPattern.Match(value);

                labelValues.Add(match.Success ? GetLabel(value) : value);
            }

            return labelValues;
        }

        public IEnumerable<string> ResolveTimeValue(Statement[] statement)
        {
            if (statement == null)
                return new List<string> { Constants.Missing };

            return statement.Select(x => GetTimeValue(x.value.content));
        }

        private string GetTimeValue(object content)
        {
            var timeProperty = ((JContainer)content).Where(p => ((JProperty)p).Name == "time").FirstOrDefault();

            if (timeProperty == null)
                return Constants.Missing;

            return ((JValue)((JProperty)timeProperty).Value).Value.ToString();
        }

        public string GetTextValue(LanguageCodes codes)
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

        private string GetLabel(string id)
        {
            JObject jsonObject = GetEntityData(id, "labels").Result;

            var codes = jsonObject.ToObject<LanguageCodes>();

            if (codes.en != null)
                return codes.en;

            var value = GetValueOfFirstFilledProperty(codes);

            if (value != null)
                return value;

            return jsonObject.Count == 0 ? Constants.Missing : ((JValue)((JProperty)jsonObject.First).Value).Value.ToString();
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
