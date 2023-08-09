using WikidataEditor.Dtos.CoreData;
using WikidataEditor.Models.Instances;

namespace WikidataEditor.Services
{
    public interface IMappingService
    {
        WikidataItemAstronomicalObjectTypeDto MapToAstronomicalObjectTypeDto(WikidataItemBaseDto basicData, WikidataItemOnAstronomicalObjectTypes item);
        WikidataItemDisambiguationPageDto MapToDisambiguationPageDto(WikidataItemBaseDto basicData, WikidataItemOnDisambiguationPages item);
        WikidataItemHumanDto MapToHumanDto(WikidataItemBaseDto basicData, WikidataItemOnHumans item);
    }
}