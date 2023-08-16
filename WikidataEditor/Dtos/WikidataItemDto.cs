
using WikidataEditor.Dtos.Requests;
using WikidataEditor.Models;

namespace WikidataEditor.Dtos
{
    /// <summary>
    /// Dto of a Wikidata item
    /// </summary>
    public class WikidataItemDto
    {
        public WikidataItemDto(string id, string @type, Dictionary<string, List<string>> aliases)
        {
            Id = id;
            Type = @type;
            Aliases = aliases;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public IEnumerable<EntityTextDto> Labels { get; set; }
        public IEnumerable<EntityTextDto> Descriptions { get; set; }
        public Dictionary<string, List<string>> Aliases { get; set; }
        public List<Sitelink?> Sitelinks { get; set; }
    }
}