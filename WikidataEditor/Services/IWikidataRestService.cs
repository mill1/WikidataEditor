﻿using WikidataEditor.Dtos;

namespace WikidataEditor.Services
{
    public interface IWikidataRestService
    {
        IWikidataItemDto GetData(string id);
    }
}