namespace WikidataEditor.Dtos
{
    // https://www.wikidata.org/wiki/Help:Properties

    public class WikidataStatementsDto
    {
        /// <summary>
        /// The unique identifier of the -item        
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Property P31
        /// </summary>
        public bool IsHuman { get; set; }

        /// <summary>
        /// Property P21
        /// </summary>
        public IEnumerable<string> SexOrGender { get; set; }

        /// <summary>
        /// Property P27
        /// </summary>
        public IEnumerable<string> CountryOfCitizenship { get; set; }

        /// <summary>
        /// Property P735
        /// </summary>
        public IEnumerable<string> GivenName { get; set; }

        /// <summary>
        /// Property P734
        /// </summary>
        public IEnumerable<string> FamilyName { get; set; }

        /// <summary>
        /// Property P569
        /// </summary>
        public IEnumerable<string> DateOfBirth { get; set; }

        /// <summary>
        /// Property P19
        /// </summary>
        public IEnumerable<string> PlaceOfBirth { get; set; }

        /// <summary>
        /// Property P570
        /// </summary>
        public IEnumerable<string> DateOfDeath { get; set; }

        /// <summary>
        /// Property P20
        /// </summary>
        public IEnumerable<string> PlaceOfDeath { get; set; }
    }
}
