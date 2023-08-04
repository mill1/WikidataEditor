namespace WikidataEditor.Dtos
{
    public class UriCollectionDto
    {
        public string WikidataUri { get; set; }
        public List<string> Wikipedias { get; set; }
        public List<string> InstanceUris { get; set; } = new();
    }
}
