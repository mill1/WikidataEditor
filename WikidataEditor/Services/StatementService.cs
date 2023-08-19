using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net;
using WikidataEditor.Common;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;
using Wikimedia.Utilities.Interfaces;
using Wikimedia.Utilities.Services;

namespace WikidataEditor.Services
{
    public class StatementService
    {
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

        public void UpsertStatementDoDWikipediaAsync(string articleTitle, string id, DateOnly dateOfDeath)
        {
            // Check if the DoD is present in the item's wiki bio.
            string wikiText = _wikipediaWebClient.GetWikiTextArticle(articleTitle, out string redirectedArticle);

            // Redirect?
            if (redirectedArticle != null)
                throw new HttpRequestException($"'{articleTitle}' results in a redirect to '{redirectedArticle}", null, HttpStatusCode.BadRequest);

            var dateOfDeathInArticle = _wikiTextService.ResolveDate(wikiText, dateOfDeath.ToDateTime(TimeOnly.MinValue));

            if (dateOfDeathInArticle == DateTime.MinValue)
            {
                var deathDateText = dateOfDeath.ToString("d MMMM yyyy", CultureInfo.GetCultureInfo("en-US"));
                throw new HttpRequestException($"Death date {deathDateText} not encountered in article", null, HttpStatusCode.NotFound);
            }

        }

        public void UpsertStatementDoDReference(string id, DateOnly dateOfDeath, string url)
        {
            var statements = _wikidataHelper.GetStatementsAsJObject(id).Result;
            var statedInId = ResolveStatedInId(url);
            var statementsDoD = TryGetStatement(statements, Constants.PropertyIdDateOfDeath);

            if (statementsDoD == null)
            {
                AddDoDStatement(id, dateOfDeath, url, statedInId);
                return;
            }

            List<Reference> references = new();
            var statementId = ResolveStatementId(dateOfDeath, url, statementsDoD, ref references);

            if (statementId == string.Empty)
            {
                // P570 statement exists, but statement does not exist for the passed date of death
                AddDoDStatement(id, dateOfDeath, url, statedInId);
                return;
            }

            UpdateDoDStatement(id, dateOfDeath, url, statedInId, references, statementId);
        }

        private void UpdateDoDStatement(string id, DateOnly dateOfDeath, string url, string statedInId, List<Reference> references, string statementId)
        {
            // Add a new reference to the existing ones
            references.Add(CreateReference(url, statedInId));

            var requestPut = CreateUpsertStatementRequest(dateOfDeath, references, statementId);

            string uri = $"items/{id}/statements/{statementId}";
            _httpClientWikidataApi.PutAsync(uri, requestPut);
        }

        private void AddDoDStatement(string id, DateOnly dateOfDeath, string url, string statedInId)
        {
            List<Reference> references = new()
            {
                CreateReference(url, statedInId)
            };

            var requestPost = CreateUpsertStatementRequest(dateOfDeath, references);

            string uri = $"items/{id}/statements";
            _httpClientWikidataApi.PostAsync(uri, requestPost);
        }

        private static UpsertStatementRequestDto CreateUpsertStatementRequest(DateOnly dateOfDeath, List<Reference> references, string statementId = null)
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
                            time = $"+{dateOfDeath.ToString("yyyy-MM-dd")}T00:00:00Z",
                            precision = 11,
                            calendarmodel = "http://www.wikidata.org/entity/Q1985727"
                        },
                        type = "value"
                    },
                    references = references.ToArray(),
                },
                tags = new string[0],
                bot = false,
                comment = $"Added{(statementId == null ? " " : " reference to ")}date of death via [[User:Mill 1|Mill 1]]'s edit app using Wikibase REST API 0.1 OAS3"
            };
        }

        private string ResolveStatementId(DateOnly dateOfDeath, string url, JToken? statementsDoD, ref List<Reference> references)
        {
            var statementId = string.Empty;            

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
                        statementId = child["id"].ToString();
                        CheckIfReferenceExists(url, child["references"].ToObject<Reference[]>().ToList());
                        break;
                    }
                }
            }
            return statementId;
        }

        private static void CheckIfReferenceExists(string url, List<Reference> references)
        {
            if (!references.Any())
                return;

            var existingReference = references.SelectMany(r => r.parts, (reference, part) => new { reference, part })
                                                .Where(refAndPart => refAndPart.part.property.id == Constants.PropertyIdReferenceUrl) 
                                                .Where(r => r.part.value.content.ToString().Contains(url, StringComparison.OrdinalIgnoreCase));

            if (existingReference.Any())
                throw new HttpRequestException("Reference already exists", null, HttpStatusCode.BadRequest);
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

        private Reference CreateReference(string url, string statedInId)
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
           
            reference.parts = AddPart(reference, url, Constants.PropertyIdReferenceUrl); 

            if (statedInId != null)
                reference.parts = AddPart(reference, statedInId, Constants.PropertyIdStatedIn);

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
    }
}
