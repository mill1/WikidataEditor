namespace WikidataEditor.Models
{
    public interface IWikidataItem
    {
        string type { get; set; }
        LanguageCodes labels { get; set; }
        LanguageCodes descriptions { get; set; }
        Dictionary<string, List<string>> aliases { get; set; }        
        Sitelinks sitelinks { get; set; }
        string id { get; set; }
    }
}
