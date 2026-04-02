using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Middlewares;
using ChatApp.Shared.Repositories;
using ChatService.Application.Features.Chats.Commands;
using ChatService.Application.Interfaces;
using ChatService.Application.Mappings;
using ChatService.Infrastructure.DatabaseContext;
using ChatService.Infrastructure.Presence;
using ChatService.Infrastructure.Repositories;
using ChatService.Infrastructure.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using StackExchange.Redis;

namespace ChatService.API
{
    public static class ServiceCollectionExtension
    {
        public static void RegisterExtension(this WebApplicationBuilder builder)
        {
            RegisterInfrastructure(builder);
        }

        private static void RegisterInfrastructure(WebApplicationBuilder builder)
        {
            // Đăng ký MongoDB Serializer cho Guid để lưu trữ dưới dạng chuẩn
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // Setup Database PostgreSQL cho Chat Service
            builder.Services.AddDbContextPool<ChatDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register SignalR
            var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";

            // 1. Đăng ký kết nối Redis (Singleton)
            builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
                ConnectionMultiplexer.Connect(redisConnectionString));

            // 2. Đăng ký PresenceTracker map với Interface (Singleton)
            builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();

            // 3. Đăng ký SignalR kèm Redis Backplane
            builder.Services.AddSignalR()
                .AddStackExchangeRedis(redisConnectionString, options =>
                {
                    options.Configuration.ChannelPrefix = "ChatApp"; // Đặt tên prefix để không đụng hàng với app khác trong cùng Redis
                });
            // ==========================================

            // Register Authentication (JWT)
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey))
                };

                // Cấu hình để SignalR có thể nhận token từ query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        // Nếu request gọi vào Hub và có kèm token ở query string
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // Đăng ký Generic Repository
            builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<ChatDbContext>());
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Đăng ký AutoMapper (quét profile trong tầng Application)
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(ChatMappingProfile).Assembly);
            });

            // Đăng ký MediatR (quét command trong tầng Application)
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SendMessageCommand).Assembly));

            // Đăng ký fluent validation (quét validator trong tầng Application)
            builder.Services.AddValidatorsFromAssembly(typeof(SendMessageCommand).Assembly);

            // 1. Đăng ký MongoDB Settings
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

            // 2. Đăng ký Message Repository dùng cho MongoDB
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();

            // Đăng ký Global Exception Handler
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
        }
    }
}