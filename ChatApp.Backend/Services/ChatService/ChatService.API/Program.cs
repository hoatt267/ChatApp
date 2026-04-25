using ChatApp.Shared.Middlewares;
using ChatService.API;
using ChatService.API.Hubs;
using ChatService.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();

builder.RegisterExtension();

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    dbContext.Database.Migrate();

    var redis = scope.ServiceProvider.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>();
    redis.GetDatabase().KeyDelete("chat_presence");
}

app.MapHub<ChatHub>("/chatHub");
app.MapHealthChecks("/health");

app.MapControllers();
app.Run();
