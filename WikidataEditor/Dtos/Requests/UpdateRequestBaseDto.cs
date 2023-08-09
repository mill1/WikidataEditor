namespace WikidataEditor.Dtos.Requests
{
    public class UpdateRequestBaseDto
    {
        // Member names are case-sensitive, otherwise 404 (Bad request)        
        public string[] tags { get; set; }
        public bool bot { get; set; }
        public string comment { get; set; }
    }
}
