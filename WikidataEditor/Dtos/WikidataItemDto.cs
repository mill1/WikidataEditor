
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Dtos
{
    /// <summary>
    /// Dto of a Wikidata item
    /// </summary>
    public class WikidataItemDto
    {
        public WikidataItemDto(string id, string @type)
        {
            Id = id;
            Type = @type;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public IEnumerable<EntityTextDto> Labels { get; set; }
        public IEnumerable<EntityTextDto> Descriptions { get; set; }
        public Dictionary<string, List<string>> Aliases { get; set; }
        // TODO
        //public Sitelinks sitelinks { get; set; }
    }
}