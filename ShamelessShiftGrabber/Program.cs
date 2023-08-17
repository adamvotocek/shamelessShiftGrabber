using Microsoft.EntityFrameworkCore;
using ShamelessShiftGrabber.Macrodroid;
using ShamelessShiftGrabber.Repository;
using Quartz;
using ShamelessShiftGrabber;
using ShamelessShiftGrabber.Scrape;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//services.AddEndpointsApiExplorer();
//services.AddSwaggerGen();

var triggerUrl = builder.Configuration.GetValue<string>("MacrodroidTriggerBaseUrl");
services.AddHttpClient("macrodroid", c =>
{
    c.BaseAddress = new Uri(triggerUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

services.AddLogging();
//services.AddHealthChecks();

services.AddSingleton<ScrapingConfiguration>();
services.AddTransient<ScrapingService>();
services.AddTransient<ShiftRepository>();
services.AddTransient<Macrodroid>();

var connectionString = builder.Configuration.GetConnectionString("ApiDatabase");
services.AddDbContext<ShiftsDatabaseContext>(options =>
{
    options.UseMySQL(connectionString!);
});

var cronExpression = builder.Configuration.GetValue<string>("QuartzCronExpression");
services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

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

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHealthChecks("/health");
//app.UseHttpsRedirection();


// Migrate latest database changes during startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ShiftsDatabaseContext>();

    // Here is the migration executed
    dbContext.Database.Migrate();
}

// For testing purposes:
//var test = new ScrapingService();
//await test.ScrapeShifts();

app.Run();