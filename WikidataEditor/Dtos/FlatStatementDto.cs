namespace WikidataEditor.Dtos
{
    public class FlatStatementDto
    {
        public string Property { get; set; }
        public IEnumerable<string> Values { get; set; }
    }
}