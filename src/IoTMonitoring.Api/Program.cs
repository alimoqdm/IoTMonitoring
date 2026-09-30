using IoTMonitoring.Application.Interfaces;
using IoTMonitoring.Application.Services;
using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.RuleEngine;
using IoTMonitoring.Domain.RuleEngine.Strategies;
using IoTMonitoring.Infrastructure;
using IoTMonitoring.Infrastructure.Data;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddInfrastructure("Data Source=iot_monitoring.db");

builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();
builder.Services.AddScoped<IProcessingOrchestrator, ProcessingOrchestrator>();
builder.Services.AddScoped<IAggregationService, AggregationService>();

builder.Services.AddScoped<IRuleOperatorStrategy, GreaterThanStrategy>();
builder.Services.AddScoped<IRuleOperatorStrategy, SustainedAboveStrategy>();
builder.Services.AddScoped<IRuleOperatorStrategy, BetweenStrategy>();
builder.Services.AddScoped<IRuleOperatorStrategy, LessThanStrategy>();
builder.Services.AddScoped<RuleOperatorFactory>();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IoTDbContext>();
    dbContext.Database.EnsureCreated();

    var ingestionService = scope.ServiceProvider.GetRequiredService<IDataIngestionService>();
    var orchestrator = scope.ServiceProvider.GetRequiredService<IProcessingOrchestrator>();

    string rulesFilePath = "rules.json"; 
    string dataFilePath = "readings.jsonl";

    if (File.Exists(rulesFilePath) && File.Exists(dataFilePath))
    {
        var rulesJson = File.ReadAllText(rulesFilePath);
        var rules = JsonSerializer.Deserialize<List<RuleDefinition>>(rulesJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Console.WriteLine("Starting data ingestion and processing...");
        var (validReadings, report) = await ingestionService.IngestFileAsync(dataFilePath);

        report = await orchestrator.ProcessDataAsync(validReadings, rules ?? new(), report);

        Console.WriteLine("\n================ PROCESSING REPORT ================");
        Console.WriteLine($"Total lines read:           {report.TotalLinesRead}");
        Console.WriteLine($"Invalid records rejected:   {report.InvalidRecords}");
        Console.WriteLine($"Parsed valid readings:      {report.ParsedReadings}");
        Console.WriteLine($"Duplicates removed:         {report.DuplicatesRemoved}");
        Console.WriteLine("---------------------------------------------------");
        Console.WriteLine($"Rules loaded:               {rules?.Count ?? 0}");
        Console.WriteLine($"Acceptable readings:        {report.AcceptableReadings}");
        Console.WriteLine($"Unacceptable readings:      {report.UnacceptableReadings}");
        Console.WriteLine($"Stored readings:            {report.AcceptableReadings + report.UnacceptableReadings}");
        Console.WriteLine($"Alerts generated:           {report.AlertsGenerated}");
        Console.WriteLine("===================================================\n");
    }
    else
    {
        Console.WriteLine("Warning: 'rules.json' or 'readings.jsonl' not found in the root directory!");
    }
}



app.MapGet("/api/aggregation", async (
    string deviceId,
    string metric,
    DateTime from,
    DateTime to,
    int bucketSizeSeconds,
    IAggregationService aggregationService) =>
{
    var result = await aggregationService.AggregateAsync(deviceId, metric, from, to, bucketSizeSeconds);
    return Results.Ok(result);
});



app.Run();

