using Newtonsoft.Json.Linq;
using System.Net;
using WikidataEditor.Common;

namespace WikidataEditor.Services
{
    public class WikipediaApiService : IWikipediaApiService
    {
        private readonly IHttpClientEnglishWikipediaApi _clientWikipediaApi;

        public WikipediaApiService(IHttpClientEnglishWikipediaApi httpClientEnglishWikipediaApi)
        {
            _clientWikipediaApi = httpClientEnglishWikipediaApi;
        }

        public string GetWikibaseItemId(string wikipediaTitle)
        {
            // f.i. https://en.wikipedia.org/w/api.php?action=query&prop=pageprops&titles=Thomas_Taylor%2C_Baron_Taylor_of_Gryfe&format=json
            string url = $"?action=query&prop=pageprops&titles={wikipediaTitle}&format=json";
            var jsonString = _clientWikipediaApi.GetStringAsync(url).Result;

            JObject jObject = JObject.Parse(jsonString);
            JObject pagesObject = jObject["query"]["pages"].ToObject<dynamic>();

            var wikibaseItemId = TryGetWikibaseItem(pagesObject);

            return (string)((JValue)wikibaseItemId)?.Value;
        }

        private JToken? TryGetWikibaseItem(JObject pagesObject)
        {
            try
            {
                var firstPageName = ((JProperty)pagesObject.First).Name;

                var pageProperties = pagesObject[firstPageName]["pageprops"];

                if (((JProperty)pageProperties.First()).Name == "disambiguation")
                    throw new HttpRequestException("Wikibase item as a Wikimedia disambiguation page", null, HttpStatusCode.BadRequest);

                return pageProperties["wikibase_item"];
            }
            catch (Exception e)
            {
                if (e is HttpRequestException)
                    throw (HttpRequestException)e;

                return null;
            }
        }
    }
}
