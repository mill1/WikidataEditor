namespace WikidataEditor.Dtos.CoreData
{
    public class UriCollectionDto
    {
        public string WikidataUri { get; set; }
        public IEnumerable<string> Wikipedias { get; set; }
    }
}
