using Newtonsoft.Json.Linq;
using WikidataEditor.Common;
using WikidataEditor.Dtos;

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
        // Ted Rogers (Q7693675):
        // https://localhost:44351/api/items/statements?id=Q7693675&property=P570
        Check if date of death statement exists.
        if not: Add date of death statement ( + ref.); END
        if exists:
        replace statement: use the response and add the ref. data

        */

        public void UpsertStatementDoDReference(string id, string url)
        {
            var statements = _wikidataHelper.GetStatementsAsJObject(id).Result;

            var statementDoD = TryGetStatementDoD(statements);

            if(statementDoD == null) 
            {
                // TODO: Add volledig P570 statement + ref. met de datum die voorkomt in de url.
                throw new NotImplementedException("Adding a P570 statement has not been implemented yet.");
            }

            string multilineMessage = """
               {
                 "property": {
                   "id": "P813",
                   "data-type": "time"
                 },
                 "value": {
                   "type": "value",
                   "content": {
                     "time": "+2017-10-09T00:00:00Z",
                     "precision": 11,
                     "calendarmodel": "http://www.wikidata.org/entity/Q1985727"
                   }
                 }
               }
            """;


        }

 
        private JToken? TryGetStatementDoD(JObject statements)
        {
            try
            {
                //var firstPageName = ((JProperty)statements.First).Name;
                return statements["P570"];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
