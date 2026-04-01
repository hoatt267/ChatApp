using ChatService.API;
using ChatService.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.RegisterExtension();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.MapControllers();
app.Run();
