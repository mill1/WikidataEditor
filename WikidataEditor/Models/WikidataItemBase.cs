// https://www.mediawiki.org/wiki/Wikibase/DataModel

namespace WikidataEditor.Models
{
    public class WikidataItemBase : IWikidataItem
    {
        public string type { get; set; }
        public LanguageCodes labels { get; set; }
        public LanguageCodes descriptions { get; set; }
        public Dictionary<string, List<string>> aliases { get; set; }        
        public Sitelinks sitelinks { get; set; }
        public string id { get; set; }
    }
}