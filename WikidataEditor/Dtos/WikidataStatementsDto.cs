namespace WikidataEditor.Dtos
{
    // https://www.wikidata.org/wiki/Help:Properties

    public class WikidataStatementsDto
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
        /// Property https://www.wikidata.org/wiki/Property:P31
        /// </summary>
        public bool IsHuman { get; set; }

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
    }
}
