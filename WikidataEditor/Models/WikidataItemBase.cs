
namespace WikidataEditor.Models
{
    /// <summary>
    /// Model of a Wikidata item excluding the statements
    /// </summary>
    public class WikidataItemBase
    {
        public string type { get; set; }
        public LanguageCodes labels { get; set; }
        public LanguageCodes descriptions { get; set; }
        public Dictionary<string, List<string>> aliases { get; set; }
        public Sitelinks sitelinks { get; set; }
        public string id { get; set; }
    }
}