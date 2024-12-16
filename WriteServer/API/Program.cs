// var builder = WebApplication.CreateBuilder(args);


// builder.Configuration.AddUserSecrets<Program>();

// var configuration = builder.Configuration;
// builder.Services.AddSingleton(configuration);

// // Add services to the container.
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment()) {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();
// app.Run();

using Persistence;

// var session = CassandraSessionManager.GetSession();
// var rowSet = session.Execute("select * from system.local");
// Console.WriteLine(rowSet.First().GetValue<string>("key"));

var database = RedisSessionManager.GetDatabase();
var pingResponse = database.Execute("PING");
Console.WriteLine(pingResponse.ToString());