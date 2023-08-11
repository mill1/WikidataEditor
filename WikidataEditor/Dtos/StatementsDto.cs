using WikidataEditor.Models;

namespace WikidataEditor.Dtos
{
    public class StatementsDto
    {
        public string Property { get; set; }

        public Statement[] Statement { get; set; }
    }
}
