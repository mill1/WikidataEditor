namespace WikidataEditor.Models
{
    public class Statement 
    {
        public string id { get; set; }
        public string rank { get; set; }
        public Qualifier[] qualifiers { get; set; }
        public Reference[] references { get; set; }
        public Property property { get; set; }
        public Value value { get; set; }
    }
}
