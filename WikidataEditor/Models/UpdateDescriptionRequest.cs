namespace WikidataEditor.Models
{
    public class UpdateDescriptionRequest
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public bool Bot { get; set; }
        public string Comment { get; set; }
    }

}
