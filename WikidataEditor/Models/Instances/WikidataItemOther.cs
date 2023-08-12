using WikidataEditor.Dtos;

namespace WikidataEditor.Models.Instances
{
    public class WikidataItemOther : WikidataItemBase
    {
        public IEnumerable<FlatStatementDto> FlatStatements { get; set; }
    }
}
