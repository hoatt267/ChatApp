using ChatService.API;
using ChatService.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterExtension();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapHub<ChatHub>("/chatHub");

app.Run();
