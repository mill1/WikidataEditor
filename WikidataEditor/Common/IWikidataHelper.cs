﻿using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;

namespace WikidataEditor.Common
{
    public interface IWikidataHelper
    {
        Task<IEnumerable<EntityTextDto>> GetEntityTexts(string id, string entityType);
        Task<IEnumerable<EntityTextDto>> GetEntityText(string id, string languageCode, string entityType);
        string GetTextValue(LanguageCodes codes);
        IEnumerable<string> ResolveTimeValue(Statement[] statement);
        IEnumerable<string> ResolveValue(Statement[] statements);
    }
}