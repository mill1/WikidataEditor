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
                SexOrGender = _helper.ResolveValues(item.statements.P21),
                CountryOfCitizenship = _helper.ResolveValues(item.statements.P27),
                GivenName = _helper.ResolveValues(item.statements.P735),
                FamilyName = _helper.ResolveValues(item.statements.P734),
                DateOfBirth = _helper.ResolveTimeValue(item.statements.P569),
                PlaceOfBirth = _helper.ResolveValues(item.statements.P19),
                DateOfDeath = _helper.ResolveTimeValue(item.statements.P570),
                PlaceOfDeath = _helper.ResolveValues(item.statements.P20),
                Occupation = _helper.ResolveValues(item.statements.P106),
            };
        }

        public WikidataItemDisambiguationPageDto MapToDisambiguationPageDto(WikidataItemBaseDto basicData, WikidataItemOnDisambiguationPages item)
        {
            return new WikidataItemDisambiguationPageDto(basicData)
            {
                DifferentFrom = _helper.ResolveValues(item.statements.P1889),
                PartiallyCoincidentWith = _helper.ResolveValues(item.statements.P1382),
                SaidToBeTheSameAs = _helper.ResolveValues(item.statements.P460),
            };
        }

        public WikidataItemAstronomicalObjectTypeDto MapToAstronomicalObjectTypeDto(WikidataItemBaseDto basicData, WikidataItemOnAstronomicalObjectTypes item)
        {
            return new WikidataItemAstronomicalObjectTypeDto(basicData)
            {
                SubclassOf = _helper.ResolveValues(item.statements.P279),
                PartOf = _helper.ResolveValues(item.statements.P361),
                Image = _helper.ResolveValues(item.statements.P18),
                HasUse = _helper.ResolveValues(item.statements.P366),
                AstronomicSymbolImage = _helper.ResolveValues(item.statements.P367),
                DescribedBySource = _helper.ResolveValues(item.statements.P1343),
            };
        }
    }
}
