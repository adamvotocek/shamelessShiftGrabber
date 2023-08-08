using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShamelessShiftGrabber.Apify;
using ShamelessShiftGrabber.Contracts;
using ShamelessShiftGrabber.Macrodroid;
using ShamelessShiftGrabber.Repository;
using System;

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

var apifyUrl = builder.Configuration.GetValue<string>("ApifyBaseUrl");
builder.Services.AddHttpClient("apify", c =>
{
    c.BaseAddress = new Uri(apifyUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddLogging();
builder.Services.AddHealthChecks();

builder.Services.AddTransient<Apify>();
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


// Migrate latest database changes during startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ShiftsDatabaseContext>();

    // Here is the migration executed
    dbContext.Database.Migrate();
}


app.MapPost("/shifts", async (
    [FromBody] ApifyRun incomingRun,
    Apify apify,
    ShiftRepository shiftRepository,
    Macrodroid macrodroid,
    ILogger<Program> logger
) =>
{
    if (incomingRun == null || string.IsNullOrWhiteSpace( incomingRun.RunId))
    {
        return Results.BadRequest("Apify actor run id is required");
    }

    var shifts = await apify.GetScrapedShifts(incomingRun.RunId);

    logger.LogInformation($"Received shifts: {shifts.Count}");

    if (shifts.Count> 0)
    {
        var filteredShifts = await shiftRepository.Filter(shifts);
        return await macrodroid.Send(filteredShifts);
    }

    return Results.Ok("No shifts were received or processed.");
});

app.Run();