using LiteDB;
using GameOfLifeApi.Repositories;
using GameOfLifeApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
