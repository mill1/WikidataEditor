namespace WikidataEditor.Dtos.CoreData
{
    // https://www.wikidata.org/wiki/Help:Properties

    public class FlatWikidataItemDto 
    {
        /// <summary>
        /// The unique identifier of the wikidata-item        
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The label of the wikidata-item        
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The description of the wikidata-item        
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Number of top level statements on the wikidata-item 
        /// </summary>
        public int TotalNumberOfStatements { get; set; }

        /// <summary>
        /// The flattened statements on the wikidata-item 
        /// </summary>
        public IEnumerable<FlatStatementDto> Statements { get; set; }

        /// <summary>
        /// The aliases of the wikidata-item        
        /// </summary>
        public IEnumerable<string> Aliases { get; set; }

        /// <summary>
        /// Collection of Uri's on the wikidata-item
        /// </summary>
        public UriCollectionDto UriCollection { get; set; }
    }
}
