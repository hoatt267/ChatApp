using ChatApp.Shared.Middlewares;
using ChatService.API;
using ChatService.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.RegisterExtension();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var redis = scope.ServiceProvider.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>();
    redis.GetDatabase().KeyDelete("chat_presence");
}

app.MapHub<ChatHub>("/chatHub");

app.MapControllers();
app.Run();
