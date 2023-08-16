namespace WikidataEditor.Services
{
    public interface IWikipediaApiService
    {
        string GetWikibaseItemId(string wikipediaTitle);
    }
}