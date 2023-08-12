namespace WikidataEditor.Common
{
    public static class WikidataProperties
    {
        // TODO : property name : https://www.wikidata.org/wiki/Wikidata:Database_reports/List_of_properties/all
        public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
            {
                { "P31", "instance of" },
                { "P21", "sex or gender" },
                { "P27", "country of citizenship" },
                { "P735", "given name" },
                { "P734", "family name" },
                { "P569", "date of birth" },
                { "P19", "place of birth" },
                { "P570", "date of death" },
                { "P20", "place of death" },
                //{ "P106", "occupation" },
                // etc.
            };
    }
}
