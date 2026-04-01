var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Đăng ký dịch vụ YARP vào Dependency Injection container
// LoadFromConfig sẽ đọc các cấu hình định tuyến từ file appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("CorsPolicy");

// Kích hoạt Middleware của YARP để nó bắt đầu lắng nghe và chuyển tiếp request
app.MapReverseProxy();

app.Run();