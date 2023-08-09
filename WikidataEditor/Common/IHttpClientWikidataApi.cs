namespace WikidataEditor.Common
{
    public interface IHttpClientWikidataApi
    {
        Task<string> GetStringAsync(string uri);
        Task PutAsync(string uri, object request);
    }
}