using ApplicationLogic;
using Persistence.DocumentRepository;
using Persistence.UserRepository;
using Persistence.WorkspaceRepository;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

var configuration = builder.Configuration;
builder.Services.Configure<AppSettings>(configuration);

builder.Services.AddSingleton<RedLockManager>();
builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<DocumentRepositoryCassandra>();
builder.Services.AddScoped<DocumentRepositoryRedis>();
builder.Services.AddScoped<UserRepositoryCassandra>();
builder.Services.AddScoped<UserRepositoryRedis>();
builder.Services.AddScoped<WorkspaceRepositoryCassandra>();
builder.Services.AddScoped<WorkspaceRepositoryRedis>();

builder.Services.AddScoped<UpdatesLogic>();
builder.Services.AddScoped<UserLogic>();
builder.Services.AddScoped<DocumentLogic>();
builder.Services.AddScoped<LockLogic>();
builder.Services.AddScoped<WorkspaceLogic>();

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

//var redLockManager = app.Services.GetRequiredService<RedLockManager>(); //PROVERA DA LI INICIJALIZUJE LEPO REDLOCK - RADI


if (app.Environment.IsDevelopment()) {
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseCors("CORS");
app.UseHttpsRedirection();

app.MapControllers();
app.UseWebSockets();

app.Run();
