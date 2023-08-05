using WikidataEditor.Models;

namespace WikidataEditor.Dtos
{
    public class WikidataItemOtherDto : WikidataItemBaseDto
    {
        public WikidataItemOtherDto(WikidataItemBaseDto wikidataItemBase) : base(wikidataItemBase)
        {
        }

        public Statement[] P31 { get; set; }
    }
}
