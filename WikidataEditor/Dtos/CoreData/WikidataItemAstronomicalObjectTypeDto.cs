namespace WikidataEditor.Dtos.CoreData
{
    // https://www.wikidata.org/wiki/Help:Properties

    public class WikidataItemAstronomicalObjectTypeDto : WikidataItemBaseDto
    {
        public WikidataItemAstronomicalObjectTypeDto(WikidataItemBaseDto wikidataItemBase) : base(wikidataItemBase)
        {
        }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P279
        /// </summary>
        public IEnumerable<string> SubclassOf { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P361
        /// </summary>
        public IEnumerable<string> PartOf { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P18
        /// </summary>
        public IEnumerable<string> Image { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P366
        /// </summary>
        public IEnumerable<string> HasUse { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P367
        /// </summary>
        public IEnumerable<string> AstronomicSymbolImage { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P1343
        /// </summary>
        public IEnumerable<string> DescribedBySource { get; set; }
    }
}
