using WikidataEditor.Common;
using WikidataEditor.Dtos.CoreData;
using WikidataEditor.Models.Instances;

namespace WikidataEditor.Services
{
    public class MappingService : IMappingService
    {
        private readonly IWikidataHelper _helper;

        public MappingService(IWikidataHelper wikidataHelper)
        {
            _helper = wikidataHelper;
        }

        public WikidataItemHumanDto MapToHumanDto(WikidataItemBaseDto basicData, WikidataItemOnHumans item)
        {
            return new WikidataItemHumanDto(basicData)
            {
                SexOrGender = _helper.ResolveValue(item.statements.P21),
                CountryOfCitizenship = _helper.ResolveValue(item.statements.P27),
                GivenName = _helper.ResolveValue(item.statements.P735),
                FamilyName = _helper.ResolveValue(item.statements.P734),
                DateOfBirth = _helper.ResolveTimeValue(item.statements.P569),
                PlaceOfBirth = _helper.ResolveValue(item.statements.P19),
                DateOfDeath = _helper.ResolveTimeValue(item.statements.P570),
                PlaceOfDeath = _helper.ResolveValue(item.statements.P20),
                Occupation = _helper.ResolveValue(item.statements.P106),
            };
        }

        public WikidataItemDisambiguationPageDto MapToDisambiguationPageDto(WikidataItemBaseDto basicData, WikidataItemOnDisambiguationPages item)
        {
            return new WikidataItemDisambiguationPageDto(basicData)
            {
                DifferentFrom = _helper.ResolveValue(item.statements.P1889),
                PartiallyCoincidentWith = _helper.ResolveValue(item.statements.P1382),
                SaidToBeTheSameAs = _helper.ResolveValue(item.statements.P460),
            };
        }

        public WikidataItemAstronomicalObjectTypeDto MapToAstronomicalObjectTypeDto(WikidataItemBaseDto basicData, WikidataItemOnAstronomicalObjectTypes item)
        {
            return new WikidataItemAstronomicalObjectTypeDto(basicData)
            {
                SubclassOf = _helper.ResolveValue(item.statements.P279),
                PartOf = _helper.ResolveValue(item.statements.P361),
                Image = _helper.ResolveValue(item.statements.P18),
                HasUse = _helper.ResolveValue(item.statements.P366),
                AstronomicSymbolImage = _helper.ResolveValue(item.statements.P367),
                DescribedBySource = _helper.ResolveValue(item.statements.P1343),
            };
        }
    }
}
