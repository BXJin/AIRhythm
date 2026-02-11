using AIRhythmServerApi.Models;
using AIRhythmServerApi.Services;
using AIRhythmServerApi.Stores;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Storage Options
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.AddSingleton<StorageInitializer>();

// Stores
builder.Services.AddSingleton<IJobStore, InMemoryJobStore>();
builder.Services.AddSingleton<IChartStore, InMemoryChartStore>();

// Queue + Worker
builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();
builder.Services.AddHostedService<JobWorkerService>();
builder.Services.AddSingleton<WorkerRunner>();
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));
builder.Services.AddSingleton<LyriaRunner>();
builder.Services.Configure<LyriaOptions>(builder.Configuration.GetSection("Lyria"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<StorageInitializer>();
    initializer.EnsureDirectories();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
