namespace WikidataEditor.Common
{
    public interface IHttpClientEnglishWikipediaApi
    {
        Task<string> GetStringAsync(string uri);
    }
}