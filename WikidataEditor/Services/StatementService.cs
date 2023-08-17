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
        // http://localhost:38583//api/items/statements?id=Q3484538&property=P570
        Check if date of death statement exists.
        if not: Add date of death statement ( + ref.); END
        if exists:
        replace statement: use the response and add the ref. data

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

                        // Maybe store when de

                        var refs = child["references"].ToObject<Reference[]>();

                        if (refs == null)
                            continue;                        
                            

                        foreach (var reference in child["references"])
                        {
                            foreach (var part in reference["parts"])
                            {
                                if ((string)part["property"]["id"] == "P248") // 'stated in'
                                {
                                    if ((string)part["value"]["content"] == "Q11148") // 'The Guardian'
                                        throw new HttpRequestException("Reference already exists", null, HttpStatusCode.BadRequest);
                                }
                            }
                        }
                    }
                }            
            }

            var request = new UpdateStatementRequestDto
            {
                statement = new Statement
                {
                    rank = "normal",
                    property = new Property { id = "P570"},
                    value = new Value
                    {
                        
                    }

                },
                tags = new string[0],
                bot = false,
                comment = "Testing add statement via rest API"
            };

            /*
            string uri = $"items/{id}/labels/{languageCode}";
            await _httpClientWikidataApi.PutAsync(uri, request);
            */


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
