using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("macrodroid", c =>
{
    c.BaseAddress = new Uri("https://trigger.macrodroid.com/");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/hello", () => "Hello World!");

app.MapPost("/shifts", async ([FromBody] ShiftRequest shiftRequest, IHttpClientFactory _httpClientFactory, IConfiguration _configuration) =>
{
    Console.WriteLine(shiftRequest.UserId);

    var macrodroidDeviceId = _configuration.GetValue<string>("MacrodroidDeviceId");

    var url = $"{macrodroidDeviceId}/gug";

    var client = _httpClientFactory.CreateClient("macrodroid");
    var response = await client.GetAsync(url);
    Console.WriteLine(response.StatusCode);



    return shiftRequest;
});

app.Run();



class ShiftRequest
{
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
    public string Resource { get; set; }
}

/*
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
*/