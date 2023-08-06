namespace WikidataEditor.Models
{
    public interface IUriStatements
    {
        // This statement will be used to generate a url to the BNF article on it.
        Statement[] P268 { get; set; }
    }
}
