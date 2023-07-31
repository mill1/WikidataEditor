namespace WikidataEditor.Models
{
    public class Property
    {
        public string id { get; set; }
        public string datatype { get; set; }
    }

    public class Value
    {
        public string type { get; set; }
        public object content { get; set; }
    }

    public class Reference
    {
        public string hash { get; set; }
        public Part[] parts { get; set; }
    }

    public class Part
    {
        public Property property { get; set; }
        public Value value { get; set; }
    }

    public class Qualifier
    {
        public Property property { get; set; }
        public Value value { get; set; }
    }
}