namespace WikidataEditor.Dtos
{
    public class UpdateDescriptionRequestDto
    {
        // Member names are case-sensitive, otherwise 404 (Bad request)
        public string description { get; set; }
        public string[] tags { get; set; }
        public bool bot { get; set; }
        public string comment { get; set; }
    }
}
