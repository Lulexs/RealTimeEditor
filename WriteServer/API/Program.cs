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

builder.Services.AddCors(opt => {
    opt.AddPolicy("CORS", policy => {
        policy.AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173");
    });
});
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CORS");
app.UseHttpsRedirection();

app.MapControllers();
app.UseWebSockets();

app.Run();
