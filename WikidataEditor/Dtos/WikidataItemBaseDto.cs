namespace WikidataEditor.Dtos
{
    // https://www.wikidata.org/wiki/Help:Properties

    public class WikidataItemBaseDto
    {
        public WikidataItemBaseDto()
        {
        }

        public WikidataItemBaseDto(WikidataItemBaseDto wikidataItemBase)
        {
            Id = wikidataItemBase.Id;
            Label = wikidataItemBase.Label;
            Description = wikidataItemBase.Description;
            StatementsCount = wikidataItemBase.StatementsCount;
            InstanceOf = wikidataItemBase.InstanceOf;
            Aliases = wikidataItemBase.Aliases;
            UriCollection = wikidataItemBase.UriCollection;
        }

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
        public int StatementsCount { get; set; }

        /// <summary>
        /// The top level type of instance of the wikidata-item        
        /// </summary>
        public IEnumerable<string> InstanceOf { get; set; }

        public IEnumerable<string> Aliases { get; set; }

        /// <summary>
        /// Collection of Uri's on the wikidata-item
        /// </summary>
        public UriCollectionDto UriCollection { get; set; }
    }
}
