/*
 * Wikibase REST API  0.1  OAS3
 * https://doc.wikimedia.org/Wikibase/master/js/rest-api/
 */

using WikidataEditor.Interfaces;
using WikidataEditor.Middleware;
using WikidataEditor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<IWikidataService, WikidataService>();
builder.Services.AddScoped<IStatementService, StatementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
