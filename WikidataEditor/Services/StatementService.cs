using Newtonsoft.Json.Linq;
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

            if(statementsDoD == null) 
            {
                // TODO: Add volledige P570 statement + ref. met meegegeven datum 
                throw new NotImplementedException("Adding a P570 statement has not been implemented yet.");
            }            

            bool existingValueDoD = false;
            var statementId = string.Empty;
            List<Reference> references = new();

            foreach (var child in statementsDoD)
            {
                var time = TryGetDoD(child);

                if (time == null)
                    continue;

                var dateString = ((string)time).Substring(1,10);
                DateOnly date;
                if (DateOnly.TryParse(dateString, out date))
                {
                    if (date == dateOfDeath)
                    {
                        existingValueDoD = true;
                        statementId = child["id"].ToString();
                        references = child["references"].ToObject<Reference[]>().ToList();

                        if (!references.Any())
                            break;
                       
                        // TODO: dit moet beter kunnen
                        var part = references.Where(r => r.parts.Any(p => p.property.id == "P248")) // "P248") =  'stated in'
                            .Select(r => r.parts.Where(p => p.property.id == "P248")).FirstOrDefault();

                        if (part != null)
                            if (part.First().value.content.ToString() == "Q11148") // "Q11148" = 'The Guardian'
                                throw new HttpRequestException("Reference already exists", null, HttpStatusCode.BadRequest);
                    }
                }            
            }

            references.Add(new Reference
            {
                parts = new Part[]
                {
                    new Part
                    {
                        property = new Property
                        {
                            id = "P248", // stated in
                            datatype = "wikibase-item"
                        },
                        value = new Value
                        {
                            type = "value",
                            content = "Q11148" // The Guardian
                        }
                    },
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
                    new Part
                    {
                        property = new Property
                        {
                            id = "P854", // url
                            datatype = "wikibase-item"
                        },
                        value = new Value
                        {
                            type = "value",
                            content = url
                        }
                    }
                }
            });

            // TODO: if not present also add ref to English wikipedia (= Q328). Plus retrieved?
            /*
            references.Add(new Reference
            {
                parts = new Part[]
                {
                    new Part
                    {
                        property = new Property
                        {
                            id = "P143",
                            datatype = "wikibase-item"
                        },
                        value = new Value
                        {
                            type = "value",
                            content = "Q328"
                        }
                    }
                }
            });
            */

            var request = new UpdateStatementRequestDto
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
                comment = "Added statement via [[User:Mill 1|Mill 1]]'s edit app using Wikibase REST API  0.1  OAS3"
            };

            string uri = $"items/{id}/statements/{statementId}";
            _httpClientWikidataApi.PutAsync(uri, request);
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
