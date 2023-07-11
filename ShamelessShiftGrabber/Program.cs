using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShamelessShiftGrabber;
using ShamelessShiftGrabber.Macrodroid;
using ShamelessShiftGrabber.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var triggerUrl = builder.Configuration.GetValue<string>("MacrodroidTriggerBaseUrl");
builder.Services.AddHttpClient("macrodroid", c =>
{
    c.BaseAddress = new Uri(triggerUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddLogging();
builder.Services.AddHealthChecks();

builder.Services.AddTransient<ShiftRepository>();
builder.Services.AddTransient<Macrodroid>();

var connectionString = builder.Configuration.GetConnectionString("ApiDatabase");
builder.Services.AddDbContext<ShiftsDatabaseContext>(options =>
{
    options.UseMySQL(connectionString!);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();



app.MapPost("/shifts", async (
    [FromBody] IncomingShiftRequest shiftRequest,
    ShiftRepository shiftRepository,
    Macrodroid macrodroid,
    ILogger<Program> logger
) =>
{
    if (shiftRequest == null || shiftRequest.Shifts == null)
    {
        return Results.BadRequest("Shifts are required");
    }

    logger.LogInformation($"Received shifts: {shiftRequest.Shifts.Length}");

    if (shiftRequest.Shifts.Length > 0)
    {
        var filteredShifts = await shiftRepository.Filter(shiftRequest.Shifts);
        return await macrodroid.Send(filteredShifts);
    }

    return Results.Ok("No shifts were received or processed.");
});

app.Run();