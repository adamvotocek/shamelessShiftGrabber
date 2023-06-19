using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
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

app.MapPost("/shifts", async (
    [FromBody] ShiftRequest shiftRequest,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration
    ) =>
{
    if (shiftRequest == null || shiftRequest.Shifts == null)
    {
        throw new ArgumentNullException("ShiftRequest or Shifts cannot be null");
    }

    Console.WriteLine(shiftRequest.Shifts.Length);

    if (shiftRequest.Shifts.Length > 0)
    {
        var macrodroid = new Macrodroid(httpClientFactory, configuration);
        await macrodroid.Send(shiftRequest.Shifts);
    }

    return shiftRequest;
});

app.Run();


class ShiftRequest
{
    public Shift[] Shifts { get; set; }
}

class Shift
{
    public string Name { get; set; }
    public string ShiftDate { get; set; }
    public string ShiftTime { get; set; }
    public string Place { get; set; }
    public string Role { get; set; }
    public string Occupancy { get; set; }
}

class Macrodroid
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Macrodroid(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task Send(Shift[] shifts)
    {
 
        var macrodroidDeviceId = _configuration.GetValue<string>("MacrodroidDeviceId");

        var url = $"{macrodroidDeviceId}/gug";
        var client = _httpClientFactory.CreateClient("macrodroid");

       foreach (var shift in shifts)
        {
            //await SendSingle(shift);
        }
        
        var response = await client.GetAsync(url);

        Console.WriteLine(response.StatusCode);
    }
}