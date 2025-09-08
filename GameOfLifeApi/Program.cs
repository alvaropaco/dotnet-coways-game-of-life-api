using LiteDB;
using GameOfLifeApi.Repositories;
using GameOfLifeApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
        policy
            .WithOrigins(
                "http://localhost:5178",
                "http://localhost:5179",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// Data directory & LiteDB
var dataDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "GameOfLife.db");
builder.Services.AddSingleton<ILiteDatabase>(_ => new LiteDatabase($"Filename={dbPath};Connection=shared"));

// DI
builder.Services.AddSingleton<IBoardRepository, LiteDbBoardRepository>();
builder.Services.AddSingleton<IGameOfLifeService, GameOfLifeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS before redirection so preflight (OPTIONS) is handled correctly
app.UseCors(corsPolicy);

// Avoid HTTPS redirection inside containers (only HTTP is configured there)
var runningInContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
if (!runningInContainer)
{
    app.UseHttpsRedirection();
}

// Handle any OPTIONS preflight generically and apply CORS
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok())
   .RequireCors(corsPolicy);

app.MapControllers();
app.Run();

// Enable WebApplicationFactory in tests
public partial class Program { }
