using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using WikidataEditor.Common;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;
using Wikimedia.Utilities.Interfaces;


namespace WikidataEditor.Services
{
    public class StatementService
    {
        private const string ItemIdEnglishWikipedia = "Q328";

        private readonly IHttpClientWikidataApi _httpClientWikidataApi;
        private readonly IWikidataHelper _wikidataHelper;
        private readonly IWikiTextService _wikiTextService;
        private readonly IWikipediaWebClient _wikipediaWebClient;

        public StatementService(IHttpClientWikidataApi httpClientWikidataApi, IWikidataHelper wikidataHelper, IWikiTextService wikiTextService, IWikipediaWebClient wikipediaWebClient)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
            _wikidataHelper = wikidataHelper;
            _wikiTextService = wikiTextService;
            _wikipediaWebClient = wikipediaWebClient;
        }

        public async Task<IEnumerable<StatementsDto>> Get(string id)
        {
            return await _wikidataHelper.GetStatements(id);
        }

        public async Task<IEnumerable<StatementsDto>> Get(string id, string property)
        {
            return await _wikidataHelper.GetStatement(id, property);
        }

        public void UpsertStatementDoDWikipedia(string articleTitle, string id, DateOnly dateOfDeath)
        {
            PerformWikipediaChecks(articleTitle, dateOfDeath);

            var statements = _wikidataHelper.GetStatementsAsJObject(id).Result;
            var statementsDoD = TryGetStatement(statements, Constants.PropertyIdDateOfDeath);

            if (statementsDoD == null)
            {
                AddDoDStatementWithWikipediaReference(id, dateOfDeath);
                return;
            }

            List<Reference> references = new();
            string statementId;
            ResolveExistingStatementInfo(dateOfDeath, ItemIdEnglishWikipedia, Constants.PropertyIdImportedFromWikimediaProject, statementsDoD, ref references, out statementId);

            if (statementId == string.Empty)
            {
                // P570 statement exists, but statement does not exist for the passed date of death
                AddDoDStatementWithWikipediaReference(id, dateOfDeath);
                return;
            }

            var newReference = CreateWikipediaReference();
            UpdateDoDStatementWithReference(id, dateOfDeath, newReference, references, statementId);
            RemoveOtherWikipediaReferences(id, dateOfDeath, statementsDoD);
        }

        /// <summary>
        /// Remove the Wikipedia reference(s), if any, regarding other dates of death statements
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dateOfDeath"></param>
        /// <param name="statementsDoD"></param>
        private void RemoveOtherWikipediaReferences(string id, DateOnly dateOfDeath, JToken? statementsDoD)
        {
            foreach (var child in statementsDoD)
            {
                var time = TryGetDoD(child);

                if (time == null)
                    continue;

                string existingDateString = time.ToString();

                if (existingDateString != DateOfDeathToString(dateOfDeath))
                {
                    // Get the references for this this day
                    var references = child["references"].ToObject<Reference[]>().ToList();

                    var existingWikipediaReferences = GetExistingReferencesByProperty(ItemIdEnglishWikipedia, Constants.PropertyIdImportedFromWikimediaProject, references);

                    if (!existingWikipediaReferences.Any())
                        break;

                    // Remove the reference from the existing ones
                    references.Remove(existingWikipediaReferences.First().Reference);

                    var statementId = child["id"].ToString();
                    var requestPut = CreateUpsertStatementRequest(existingDateString, references, "Removed Wikipedia reference from date of death", statementId);

                    string uri = $"items/{id}/statements/{statementId}";
                    _httpClientWikidataApi.PutAsync(uri, requestPut);
                }
            }
        }

        public void UpsertStatementDoDReference(string id, DateOnly dateOfDeath, string url)
        {
            var statedInId = ResolveStatedInId(url);

            var statements = _wikidataHelper.GetStatementsAsJObject(id).Result;
            var statementsDoD = TryGetStatement(statements, Constants.PropertyIdDateOfDeath);

            if (statementsDoD == null)
            {
                AddDoDStatementWithUrlReference(id, dateOfDeath, url, statedInId);
                return;
            }

            List<Reference> references = new();
            string statementId;
            ResolveExistingStatementInfo(dateOfDeath, url, Constants.PropertyIdReferenceUrl, statementsDoD, ref references, out statementId);

            if (statementId == string.Empty)
            {
                // P570 statement exists, but statement does not exist for the passed date of death
                AddDoDStatementWithUrlReference(id, dateOfDeath, url, statedInId);
                return;
            }

            var newReference = CreateUrlReference(url, statedInId);

            UpdateDoDStatementWithReference(id, dateOfDeath, newReference, references, statementId);
        }

        private void UpdateDoDStatementWithReference(string id, DateOnly dateOfDeath, Reference newReference, List<Reference> references, string statementId)
        {
            // Add a new reference to the existing ones
            references.Add(newReference);

            var requestPut = CreateUpsertStatementRequest(DateOfDeathToString(dateOfDeath), references, "Added reference to date of death", statementId);

            string uri = $"items/{id}/statements/{statementId}";
            _httpClientWikidataApi.PutAsync(uri, requestPut);
        }

        private void AddDoDStatementWithWikipediaReference(string id, DateOnly dateOfDeath)
        {
            List<Reference> references = new()
            {
                CreateWikipediaReference()
            };

            var requestPost = CreateUpsertStatementRequest(DateOfDeathToString(dateOfDeath), references, "Added date of death statement");

            string uri = $"items/{id}/statements";
            _httpClientWikidataApi.PostAsync(uri, requestPost);
        }

        private void AddDoDStatementWithUrlReference(string id, DateOnly dateOfDeath, string url, string statedInId)
        {
            List<Reference> references = new()
            {
                CreateUrlReference(url, statedInId)
            };

            var requestPost = CreateUpsertStatementRequest(DateOfDeathToString(dateOfDeath), references, "Added date of death statement");

            string uri = $"items/{id}/statements";
            _httpClientWikidataApi.PostAsync(uri, requestPost);
        }

        private static UpsertStatementRequestDto CreateUpsertStatementRequest(string dateOfDeathString, List<Reference> references, string comment, string statementId = null)
        {
            return new UpsertStatementRequestDto
            {
                statement = new Statement
                {
                    id = statementId,
                    rank = "normal",
                    property = new Property
                    {
                        id = Constants.PropertyIdDateOfDeath
                    },
                    value = new Value
                    {
                        content = new TimeContent
                        {
                            time = dateOfDeathString,
                            precision = 11,
                            calendarmodel = "http://www.wikidata.org/entity/Q1985727"
                        },
                        type = "value"
                    },
                    references = references.ToArray(),
                },
                tags = new string[0],
                bot = false,
                comment = $"{comment} via [[User:Mill 1]]'s edit app using Wikibase REST API 0.1 OAS3"
            };
        }

        private static string DateOfDeathToString(DateOnly dateOfDeath)
        {
            return $"+{dateOfDeath.ToString("yyyy-MM-dd")}T00:00:00Z";
        }

        private void ResolveExistingStatementInfo(DateOnly dateOfDeath, string content, string propertyId, JToken? statementsDoD,
                        ref List<Reference> references, out string statementId)
        {
            statementId = string.Empty;

            foreach (var child in statementsDoD)
            {
                var time = TryGetDoD(child);

                if (time == null)
                    continue;

                var dateString = ((string)time).Substring(1, 10);
                DateOnly date;
                if (DateOnly.TryParse(dateString, out date))
                {
                    if (date == dateOfDeath)
                    {
                        // Get the existing references for this this day
                        references = child["references"].ToObject<Reference[]>().ToList();

                        var existingReferences = GetExistingReferencesByProperty(content, propertyId, references);

                        if (existingReferences.Any())
                            throw new HttpRequestException("Reference already exists", null, HttpStatusCode.BadRequest);

                        statementId = child["id"].ToString();
                        break;
                    }
                }
            }
        }

        private static IEnumerable<FlattenedReference> GetExistingReferencesByProperty(string content, string propertyId, IEnumerable<Reference> references)
        {
            if (!references.Any())
                return new List<FlattenedReference>();

            return references.SelectMany(r => r.parts, (reference, part) => new { reference, part })
                            .Where(refAndPart => refAndPart.part.property.id == propertyId)
                            .Where(r => r.part.value.content.ToString().Contains(content, StringComparison.OrdinalIgnoreCase))
                            .Select(refAndPart =>
                                new FlattenedReference
                                {
                                    Reference = refAndPart.reference,
                                    Part = refAndPart.part
                                });
        }

        private string ResolveStatedInId(string url)
        {
            const string ItemIdTheGuardian = "Q11148";
            const string ItemIdTheIndependent = "Q11149";
            const string ItemIdTheNewYorkTimes = "Q9684";
            const string ItemIdWorldFootball = "Q20773699";
            const string ItemIdNationalFootballTeams = "Q18693731";

            if (url.Contains("theguardian.com", StringComparison.OrdinalIgnoreCase))
                return ItemIdTheGuardian;

            if (url.Contains("independent.co.uk", StringComparison.OrdinalIgnoreCase))
                return ItemIdTheIndependent;

            if (url.Contains("nytimes.com", StringComparison.OrdinalIgnoreCase))
                return ItemIdTheNewYorkTimes;

            if (url.Contains("worldfootball.net", StringComparison.OrdinalIgnoreCase))
                return ItemIdWorldFootball;

            if (url.Contains("national-football-teams.com", StringComparison.OrdinalIgnoreCase))
                return ItemIdNationalFootballTeams;

            return null;
        }       

        private Reference CreateWikipediaReference()
        {            
            Reference reference = CreateReference();
            reference.parts = AddPart(reference, ItemIdEnglishWikipedia, Constants.PropertyIdImportedFromWikimediaProject);
            
            return reference;
        }

        private Reference CreateUrlReference(string url, string statedInId)
        {
            Reference reference = CreateReference();

            reference.parts = AddPart(reference, url, Constants.PropertyIdReferenceUrl);

            if (statedInId != null)
                reference.parts = AddPart(reference, statedInId, Constants.PropertyIdStatedIn);

            return reference;
        }

        private static Reference CreateReference()
        {
            var reference = new Reference
            {
                parts = new Part[]
                {
                    new Part
                    {
                        property = new Property
                        {
                            id = Constants.PropertyIdDateRetrieved,
                            datatype = "wikibase-item"
                        },
                        value = new Value
                        {
                            type = "value",
                            content = new TimeContent // Now, corrected for UTC
                            {
                                time = $"+{DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd")}T00:00:00Z",
                                precision = 11,
                                calendarmodel = "http://www.wikidata.org/entity/Q1985727"
                            }
                        }
                    },
                }
            };
            return reference;
        }

        private static Part[] AddPart(Reference? reference, string content, string propertyId)
        {
            /* Changing array to list and back again results in invalid payload for request.
            List<Part> parts = reference.parts.ToList();
            parts.Add(CreatePart(content, content));
            return parts.ToArray();
            */

            var parts = reference.parts;
            Array.Resize(ref parts, parts.Length + 1);
            parts[parts.Length - 1] = CreatePart(content, propertyId);
            return parts;
        }

        private static Part CreatePart(string content, string propertyId)
        {
            return new Part
            {
                property = new Property
                {
                    id = propertyId,
                    datatype = "wikibase-item"
                },
                value = new Value
                {
                    type = "value",
                    content = content
                }
            };
        }

        private JToken? TryGetStatement(JObject statements, string propertyId)
        {
            try
            {
                return statements[propertyId];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private JToken? TryGetDoD(JToken? statementDoD)
        {
            try
            {
                return statementDoD["value"]["content"]["time"];
            }
            catch (Exception)
            {
                return null;
            }
        }
        private void PerformWikipediaChecks(string articleTitle, DateOnly dateOfDeath)
        {
            // Check if the DoD is present in the item's wiki bio.
            string wikiText = _wikipediaWebClient.GetWikiTextArticle(articleTitle, out string redirectedArticle);

            // Redirect?
            if (redirectedArticle != null)
                throw new HttpRequestException($"'{articleTitle}' results in a redirect to '{redirectedArticle}", null, HttpStatusCode.BadRequest);

            // "Disambiguation page?
            if (wikiText.Contains("{{hndis", StringComparison.OrdinalIgnoreCase))
                throw new HttpRequestException($"'{articleTitle}' leads to a disambiguation page", null, HttpStatusCode.BadRequest);

            var dateOfDeathInArticle = _wikiTextService.ResolveDate(wikiText, dateOfDeath.ToDateTime(TimeOnly.MinValue));

            if (dateOfDeathInArticle == DateTime.MinValue)
            {
                var deathDateText = dateOfDeath.ToString("d MMMM yyyy", CultureInfo.GetCultureInfo("en-US"));
                throw new HttpRequestException($"Death date {deathDateText} not encountered in article", null, HttpStatusCode.NotFound);
            }
        }
        private class FlattenedReference
        {
            public Reference Reference { get; set; }
            public Part Part { get; set; }
        }
    }
}
