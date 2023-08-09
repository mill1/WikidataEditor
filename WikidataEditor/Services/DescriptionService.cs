﻿using Newtonsoft.Json;
using System.Text;
using WikidataEditor.Common;
using WikidataEditor.Dtos.Requests;

namespace WikidataEditor.Services
{
    public class DescriptionService
    {
        private readonly HttpClientHelper _httpClientHelper;

        public DescriptionService(HttpClientHelper httpClientHelper)
        {
            _httpClientHelper = httpClientHelper;
        }

        public async Task UpsertDescription(string id, string description, string languageCode, string comment)
        {
            var request = new UpdateDescriptionRequestDto
            {
                description = description,
                tags = new string[0],
                bot = false,
                comment = comment
            };

            string uri = $"items/{id}/descriptions/{languageCode}";
            await _httpClientHelper.PutAsync(uri, request);            
        }
    }
}
