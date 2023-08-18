using WikidataEditor.Models;

namespace WikidataEditor.Dtos.Requests
{
    public class UpsertStatementRequestDto : UpdateRequestBaseDto
    {
        public Statement statement { get; set; }
    }
}
