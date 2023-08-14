/*
 * Wikibase REST API  0.1  OAS3
 * https://doc.wikimedia.org/Wikibase/master/js/rest-api/
 * 
 * Datamodel:
 * https://www.mediawiki.org/wiki/Wikibase/DataModel
 */

// TODO: Add statement(s)!:
// https://en.wikipedia.org/wiki/Thomas_Taylor,_Baron_Taylor_of_Gryfe
// https://www.wikidata.org/wiki/Q7794369
// https://www.theguardian.com/news/2001/jul/30/guardianobituaries1
//
// https://www.wikidata.org/wiki/Q129678
// https://en.wikipedia.org/wiki/Category:Mountains_of_Chile

using Microsoft.Net.Http.Headers;
using WikidataEditor.Common;
using WikidataEditor.Configuration;
using WikidataEditor.Middleware;
using WikidataEditor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configUring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0
builder.Services.Configure<PositionOptions>(
    builder.Configuration.GetSection(PositionOptions.Position));

builder.Services.AddHttpClient(Constants.HttpClientWikidataRestApi, httpClient =>
{
    httpClient.BaseAddress = new Uri("https://www.wikidata.org/w/rest.php/wikibase/v0/entities/");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Wikidata Editor application");
});

builder.Services.AddScoped<IHttpClientWikidataApi, HttpClientWikidataApi>();
builder.Services.AddScoped<ICoreDataService, CoreDataService>();
builder.Services.AddScoped<IWikidataHelper, WikidataHelper>();
builder.Services.AddScoped<LabelService>();
builder.Services.AddScoped<DescriptionService>();
builder.Services.AddScoped<AliasesService>();
builder.Services.AddScoped<StatementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});
app.UseStaticFiles();

app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
