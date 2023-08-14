﻿namespace WikidataEditor.Configuration
{
    public class PositionOptions
    {
        public const string Position = "Position";

        public string Title { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public IEnumerable<string> Bar { get; set; }
    }
}
