﻿namespace WikidataEditor.Models
{
    public class WikidataItem
    {
        public string type { get; set; }
        public LanguageCodes labels { get; set; }
        public LanguageCodes descriptions { get; set; }
        public Dictionary<string, List<string>> aliases { get; set; }
        public StatementsOnHumans statements { get; set; }
        public Sitelinks sitelinks { get; set; }
        public string id { get; set; }
    }

}
