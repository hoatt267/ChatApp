using ChatService.API;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterExtension();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
