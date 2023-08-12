namespace WikidataEditor.Dtos
{
    public class FlatStatementDto
    {
        public string Property { get; set; }
        public IEnumerable<string> Statement { get; set; }
    }
}