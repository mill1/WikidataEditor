using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using WikidataEditor.Models;

namespace WikidataEditor.Common
{
    public class WikidataHelper
    {
        private static readonly Regex WikidataIdPattern = new(@"Q\d{2}", RegexOptions.Compiled);
        private readonly HttpClient _client;

        public WikidataHelper(HttpClient client)
        {
            _client = client;
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
            JObject jsonObject = GetEntityData(id, "labels");

            var codes = jsonObject.ToObject<LanguageCodes>();

            if (codes.en != null)
                return codes.en;

            var value = GetValueOfFirstFilledProperty(codes);

            if (value != null)
                return value;

            return jsonObject.Count == 0 ? Constants.Missing : ((JValue)((JProperty)jsonObject.First).Value).Value.ToString();
        }

        private JObject GetEntityData(string itemId, string wikidataTypeOfData)
        {
            string Uri = "https://www.wikidata.org/w/rest.php/wikibase/v0/entities/items/" + itemId + "/" + wikidataTypeOfData;
            var jsonString = _client.GetStringAsync(Uri).Result;

            return JObject.Parse(jsonString);
        }
        
        private string GetValueOfFirstFilledProperty(LanguageCodes codes)
        {
            // Q114658910
            return codes.GetType().GetProperties()
            .Where(c => c.PropertyType == typeof(string))
            .Select(c => (string)c.GetValue(codes))
            .FirstOrDefault(value => !string.IsNullOrEmpty(value));
        }
    }
}
