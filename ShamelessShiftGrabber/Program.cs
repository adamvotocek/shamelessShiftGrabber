using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShamelessShiftGrabber.Macrodroid;
using ShamelessShiftGrabber.Repository;
using Quartz;
using ShamelessShiftGrabber;
using ShamelessShiftGrabber.Scrape;
using Serilog;
using ShamelessShiftGrabber.GoogleSheets;
using Microsoft.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;


var triggerUrl = builder.Configuration.GetValue<string>("MacrodroidTriggerBaseUrl");
services.AddHttpClient("macrodroid", c =>
{
    c.BaseAddress = new Uri(triggerUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

services.AddLogging();

services.AddSingleton<ScrapingConfiguration>();
services.AddSingleton<GoogleCredentialsConfiguration>();
services.AddSingleton<GoogleSheetConfiguration>();
services.AddSingleton<GoogleSheets>();

services.AddTransient<GoogleSheetsConditionService>();
services.AddTransient<ScrapingService>();
services.AddTransient<ShiftRepository>();
services.AddTransient<Macrodroid>();

services.AddDbContext<ShiftsDatabaseContext>();

services.TryAddSingleton<TelemetryClient>();
services.AddSingleton<AppInsightsService>();
services.AddApplicationInsightsTelemetry(builder.Configuration);

builder.Host.UseSerilog(
    (context, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    }
);

var cronExpression = builder.Configuration.GetValue<string>("QuartzCronExpression");
services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(ScheduledJob));
    q.AddJob<ScheduledJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity($"{jobKey}-trigger")
        .WithCronSchedule(cronExpression!)
    );
});
services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Migrate latest database changes during startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ShiftsDatabaseContext>();

    logger.LogDebug("Migrating database...");
    // Here is the migration executed
    dbContext.Database.Migrate();

    var shiftRepository = scope.ServiceProvider
        .GetRequiredService<ShiftRepository>();

    await shiftRepository.ReadAndLogAllTheRowsFromTheShiftsTable();
}

// For testing purposes:
//var test = new ScrapingService();
//await test.ScrapeShifts();

app.Run();
