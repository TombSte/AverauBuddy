using AverauBuddy;
using AverauBuddy.Example.CustomExceptions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails(mapper =>
{
    mapper.RegisterMapping<ForbiddenException>((ex, context) => new ProblemDetails
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        Title = $"Cannot insert new {ex.Scope} because {ex.MissingGrant} grant is missing.",
        Status = StatusCodes.Status403Forbidden,
        Detail = ex.Message,
        Instance = context.TraceIdentifier
    });
});

var app = builder.Build();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var cars = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/cars", () =>
    {
        return cars;
    })
    .WithName("GetCars")
    .WithOpenApi();

app.MapPost("/cars/{car}", ([FromRoute]string car) =>
    {
        throw new ForbiddenException("Car", "Insert");
    })
    .WithName("PostCars")
    .WithOpenApi();

app.Run();
