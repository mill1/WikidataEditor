﻿using WikidataEditor.Dtos;

namespace WikidataEditor.Interfaces
{
    public interface IWikidataService
    {
        HumanDto GetDataOnHuman(string id);
    }
}