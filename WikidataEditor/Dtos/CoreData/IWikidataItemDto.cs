namespace WikidataEditor.Dtos.CoreData
{
    public interface IWikidataItemDto
    {
        string Id { get; set; }
        string Label { get; set; }
        string Description { get; set; }
        int StatementsCount { get; set; }
        IEnumerable<string> InstanceOf { get; set; }
        IEnumerable<string> Aliases { get; set; }
        UriCollectionDto UriCollection { get; set; }
    }
}
