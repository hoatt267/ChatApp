using UserService.API;
using UserService.API.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterExtension();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<FriendshipGrpcServer>();

app.MapControllers();

app.Run();