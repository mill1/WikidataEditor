namespace WikidataEditor.Dtos
{
    // https://www.wikidata.org/wiki/Help:Properties

    public class HumanDto
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
        public int StatementsCount { get; set; }

        public IEnumerable<string> Aliases { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P21
        /// </summary>
        public IEnumerable<string> SexOrGender { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P27
        /// </summary>
        public IEnumerable<string> CountryOfCitizenship { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P735
        /// </summary>
        public IEnumerable<string> GivenName { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P734
        /// </summary>
        public IEnumerable<string> FamilyName { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P569
        /// </summary>
        public IEnumerable<string> DateOfBirth { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P19
        /// </summary>
        public IEnumerable<string> PlaceOfBirth { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P570
        /// </summary>
        public IEnumerable<string> DateOfDeath { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P20
        /// </summary>
        public IEnumerable<string> PlaceOfDeath { get; set; }

        /// <summary>
        /// Property https://www.wikidata.org/wiki/Property:P106
        /// </summary>
        public IEnumerable<string> Occupation { get; set; }

        // LibraryOfCongressAuthorityURI
        /// <summary>
        /// Collection of uri's on the human
        /// </summary>
        public URICollectionDto UriCollection { get; set; } = new();
    }
}
