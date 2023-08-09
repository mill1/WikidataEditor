namespace WikidataEditor.Dtos.CoreData
{
    // https://www.wikidata.org/wiki/Help:Properties

    public class WikidataItemDisambiguationPageDto : WikidataItemBaseDto
    {
        public WikidataItemDisambiguationPageDto(WikidataItemBaseDto wikidataItemBase) : base(wikidataItemBase)
        {
        }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P1889
        /// </summary>
        public IEnumerable<string> DifferentFrom { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P1382
        /// </summary>
        public IEnumerable<string> PartiallyCoincidentWith { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P460
        /// </summary>
        public IEnumerable<string> SaidToBeTheSameAs { get; set; }
    }
}
