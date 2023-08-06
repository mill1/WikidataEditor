using WikidataEditor.Models;

namespace WikidataEditor.Common
{
    public interface IWikidataHelper
    {
        string GetTextValue(LanguageCodes codes);
        IEnumerable<string> ResolveTimeValue(Statement[] statement);
        IEnumerable<string> ResolveValue(Statement[] statements);
    }
}