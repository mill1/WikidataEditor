using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Reflection.Emit;
using System.Xml.Linq;
using WikidataEditor.Common;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;

namespace WikidataEditor.Services
{
    public class StatementService
    {
        private readonly IHttpClientWikidataApi _httpClientWikidataApi;
        private readonly IWikidataHelper _wikidataHelper;

        public StatementService(IHttpClientWikidataApi httpClientWikidataApi, IWikidataHelper wikidataHelper)
        {
            _httpClientWikidataApi = httpClientWikidataApi;
            _wikidataHelper = wikidataHelper;
        }

        public async Task<IEnumerable<StatementsDto>> Get(string id)
        {
            return await _wikidataHelper.GetStatements(id);
        }

        public async Task<IEnumerable<StatementsDto>> Get(string id, string property)
        {
            return await _wikidataHelper.GetStatement(id, property);
        }


        // TODO: Add statement(s):
        // https://www.wikidata.org/wiki/Q129678
        // https://en.wikipedia.org/wiki/Category:Mountains_of_Chile

        /*
         Regarding auto-adding ref. The Guardian obituary to the date of death:
         https://en.wikipedia.org/wiki/Thomas_Taylor,_Baron_Taylor_of_Gryfe
         https://www.wikidata.org/wiki/Q7794369
         https://www.theguardian.com/news/2001/jul/30/guardianobituaries1

        // template existing ref The Guardian for DoD. TODO date regarding prop 'retrieved' (P813) is empty?
        // Simone Benmussa (Q3484538):
        // http://localhost:38583/api/items/statements?id=Q3484538&property=P570
        */

        public void UpsertStatementDoDReference(string id, DateOnly dateOfDeath, string url)
        {
            var statements = _wikidataHelper.GetStatementsAsJObject(id).Result;

            var statementsDoD = TryGetStatementDoD(statements);

            var statedInId = ResolveStatedInId(url);

            if (statementsDoD == null)
            {
                // TODO: Add volledige P570 statement + ref. met meegegeven datum 
                throw new NotImplementedException("Adding a P570 (=DoD) statement has not been implemented yet.");
            }

            List<Reference> references = new();
            var statementId = CheckStatementDoD(dateOfDeath, url, statementsDoD, ref references);

            if (statementId == string.Empty)
            {
                // TODO P570 statement bestaat, maar bestaat nog niet voor de meegegeven datum
                throw new NotImplementedException("Adding a day to a P570 statement has not been implemented yet.");
            }

            // Add a new reference to the existing ones
            references.Add(CreateReference(url, statedInId));

            var request = CreateUpdateStatementRequest(dateOfDeath, references, statementId);

            string uri = $"items/{id}/statements/{statementId}";
            _httpClientWikidataApi.PutAsync(uri, request);
        }

        private static UpdateStatementRequestDto CreateUpdateStatementRequest(DateOnly dateOfDeath, List<Reference> references, string statementId)
        {
            return new UpdateStatementRequestDto
            {
                statement = new Statement
                {
                    id = statementId,
                    rank = "normal",
                    property = new Property
                    {
                        id = "P570"
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
                comment = "Added reference to date of death via [[User:Mill 1|Mill 1]]'s edit app using Wikibase REST API 0.1 OAS3"
            };
        }

        private string CheckStatementDoD(DateOnly dateOfDeath, string url, JToken? statementsDoD, ref List<Reference> references)
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
                        references = child["references"].ToObject<Reference[]>().ToList();

                        if (!references.Any())
                            break;

                        var existingReference = references.SelectMany(r => r.parts, (reference, part) => new { reference, part })
                                    .Where(refAndPart => refAndPart.part.property.id == "P854") // "P854" = 'reference URL'
                                    .Where(x => x.part.value.content.ToString().Contains(url, StringComparison.OrdinalIgnoreCase));

                        if (existingReference.Any())
                            throw new HttpRequestException("Reference already exists", null, HttpStatusCode.BadRequest);
                    }
                }
            }

            return statementId;
        }

        private string ResolveStatedInId(string url)
        {
            if (url.Contains("theguardian.com", StringComparison.OrdinalIgnoreCase))
            {
                return "Q11148"; /// = 'The Guardian'
            }

            return null;
        }

        private Reference CreateReference(string url, string statedInId)
        {
            var reference =  new Reference
            {
                parts = new Part[]
                {                    
                    new Part
                    {
                        property = new Property
                        {
                            id = "P813", // retrieved
                            datatype = "wikibase-item"
                        },
                        value = new Value
                        {
                            type = "value",
                            content = new TimeContent // Now
                            {
                                time = $"+{DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd")}T00:00:00Z",
                                precision = 11,
                                calendarmodel = "http://www.wikidata.org/entity/Q1985727"
                            }
                        }
                    },
                }
            };

            reference.parts.ToList().Add(CreatePart(url, "P854")); // "P854" = reference url

            if (statedInId != null)
                reference.parts.ToList().Add(CreatePart(statedInId, "P248")); // "P248" = stated in

            return reference;
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

        private JToken? TryGetStatementDoD(JObject statements)
        {
            try
            {
                return statements["P570"];
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
