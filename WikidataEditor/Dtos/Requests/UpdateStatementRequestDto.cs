using WikidataEditor.Models;

namespace WikidataEditor.Dtos.Requests
{
    public class UpdateStatementRequestDto : UpdateRequestBaseDto
    {
        public Statement statement { get; set; }
    }
}
