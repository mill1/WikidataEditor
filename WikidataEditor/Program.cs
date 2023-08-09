/*
 * Wikibase REST API  0.1  OAS3
 * https://doc.wikimedia.org/Wikibase/master/js/rest-api/
 */

using Microsoft.Net.Http.Headers;
using WikidataEditor.Common;
using WikidataEditor.Middleware;
using WikidataEditor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configUring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient(Constants.HttpClientWikidataRestApi, httpClient =>
{
    httpClient.BaseAddress = new Uri("https://www.wikidata.org/w/rest.php/wikibase/v0/entities/");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Wikidata Editor application");
});

builder.Services.AddScoped<IWikidataRestService, WikidataRestService>();
builder.Services.AddScoped<IWikidataService, WikidataService>();
builder.Services.AddScoped<IMappingService, MappingService>();
builder.Services.AddScoped<IWikidataHelper, WikidataHelper>();
builder.Services.AddScoped<DescriptionService>();

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
