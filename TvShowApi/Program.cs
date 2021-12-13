using Domain.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IShowRepository, ShowRepository>();
builder.Services.AddScoped<IShowPageRepository, ShowPageRepository>();
builder.Services.AddSingleton<ILiteRepository>(x => new LiteRepository(builder.Configuration["ShowsDbConnectionString"]));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/shows/{id}", async ([FromRoute] int id, IShowRepository showRepository) =>
{
    var show = showRepository.GetShow(id);
    return show != null ? Results.Ok(show) : Results.NotFound();
});

app.MapGet("/shows", ([FromQuery(Name = "pageNumber")] int pageNumber,
    [FromQuery(Name = "pageSize")] int pageSize, IShowRepository showRepository) =>
{
    var shows = showRepository.GetShows(pageNumber, pageSize);
    return shows != null && shows.Any() ? Results.Ok(shows) : Results.NotFound();
});

app.MapPost("/shows", ([FromBody] Show show, IShowRepository showRepository) =>
{
    showRepository.AddShow(show);
    return Results.Created($"/shows/{show.Id}", show);
});

app.Run();

public partial class Program { }