using System.ComponentModel;

namespace WikidataEditor.Extensions
{
    public static class StringExtensions
    {
        public static DateOnly ParseDateOnlyWikidata(this string val)
        {
            DateOnly dateOnly;
            

            if(DateOnly.TryParse(val, out dateOnly))
            {
                return dateOnly;
            }
            else
            {
                string dateEdited = val.Trim();

                // 'November 1919' or '1919'
                dateEdited = dateEdited.Contains(' ') ? $"1 {dateEdited}" : $"{dateEdited}/1/1";

                if (DateOnly.TryParse(dateEdited, out dateOnly))
                {
                    return dateOnly;
                }
                else
                {
                    return DateOnly.MinValue;
                }
            }
        }
    }
}
