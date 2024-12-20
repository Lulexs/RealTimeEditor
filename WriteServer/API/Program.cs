using ApplicationLogic;
using Persistence.DocumentRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

var configuration = builder.Configuration;
builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<DocumentRepositoryCassandra>();
builder.Services.AddScoped<DocumentRepositoryRedis>();

builder.Services.AddScoped<UpdatesLogic>();

builder.Services.AddControllers();

builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.UseWebSockets();

app.Run();
