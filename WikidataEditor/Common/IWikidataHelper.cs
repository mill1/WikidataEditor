using Newtonsoft.Json.Linq;
using WikidataEditor.Dtos;
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;

namespace WikidataEditor.Common
{
    public interface IWikidataHelper
    {
        Task<IEnumerable<StatementsDto>> GetStatement(JObject statementsObject, string property);
        Task<IEnumerable<StatementsDto>> GetStatement(string id, string property);
        IEnumerable<string> GetProperties(dynamic statementsObject, int count);
        Task<IEnumerable<StatementsDto>> GetStatements(string id);
        List<FlatStatementDto> GetStatementsValues(dynamic statementsObject, IEnumerable<string> properties);
        Task<IEnumerable<EntityTextDto>> GetEntityTexts(string id, string entityType);
        Task<IEnumerable<EntityTextDto>> GetEntityText(string id, string languageCode, string entityType);
        Task<IEnumerable<EntityTextDto>> GetAliases(string id);
        string GetTextValue(LanguageCodes codes);
        IEnumerable<string> ResolveValues(Statement[] statements);
    }
}