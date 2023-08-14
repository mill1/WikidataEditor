namespace WikidataEditor.Configuration
{
    public class CoreDataOptions
    {
        public const string CoreData = "CoreData";

        public int MaxNumberOfProperties { get; set; } = 3;
        public IEnumerable<WikidataItems> WikidataItems { get; set; }
    }

    public class WikidataItems
    {
        public string WikidataItemId { get; set; }
        public IEnumerable<string> Properties { get; set; }
    }
}
