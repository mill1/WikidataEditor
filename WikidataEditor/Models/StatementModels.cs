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

    public class TimeContent
    {
        public string time { get; set; }
        public int? precision { get; set; }
        public string calendarmodel { get; set; }
    }

    public class GlobeCoordinateContent
    {
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public decimal? altitude { get; set; }
        public decimal? precision { get; set; }
        public string globe { get; set; }
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