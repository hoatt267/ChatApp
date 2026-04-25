using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Middlewares;
using ChatApp.Shared.Protos;
using ChatApp.Shared.Repositories;
using ChatApp.Shared.Services;
using ChatService.API.Services;
using ChatService.Application.EventConsumers;
using ChatService.Application.Features.Chats.Commands;
using ChatService.Application.Interfaces;
using ChatService.Application.Services;
using ChatService.Infrastructure.DatabaseContext;
using ChatService.Infrastructure.Presence;
using ChatService.Infrastructure.Repositories;
using ChatService.Infrastructure.Settings;
using FluentValidation;
using MassTransit;
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
            RegisterApplication(builder);
        }

        private static void RegisterApplication(WebApplicationBuilder builder)
        {
            // Đăng ký Enricher Service
            builder.Services.AddScoped<IConversationEnricher, ConversationEnricher>();

            // Đăng ký Blob Storage Service
            builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

            builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
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
                    options.Configuration.ChannelPrefix = RedisChannel.Literal("ChatApp"); // Đặt tên prefix để không đụng hàng với app khác trong cùng Redis
                });
            builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });

            var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitVirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
            var rabbitUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

            //Register RabbitMQ - Consumer
            builder.Services.AddMassTransit(x =>
                {
                    // 1. ĐĂNG KÝ CONSUMER
                    x.AddConsumer<UserCreatedEventConsumer>();
                    x.AddConsumer<UserUpdatedEventConsumer>();
                    x.AddConsumer<FriendshipEventConsumer>();
                    x.AddConsumer<UserBlockedEventConsumer>();
                    x.AddConsumer<UserUnblockedEventConsumer>();

                    // 2. CẤU HÌNH KẾT NỐI RABBITMQ
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(rabbitHost, rabbitVirtualHost, h =>
                        {
                            h.Username(rabbitUsername);
                            h.Password(rabbitPassword);
                        });

                        cfg.ReceiveEndpoint("chat-service-user-created", e =>
                        {
                            e.ConfigureConsumer<UserCreatedEventConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("chat-service-user-updated", e =>
                        {
                            e.ConfigureConsumer<UserUpdatedEventConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("chat-service-friendship-updated", e =>
                        {
                            e.ConfigureConsumer<FriendshipEventConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("chat-service-user-blocked", e =>
                        {
                            e.ConfigureConsumer<UserBlockedEventConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("chat-service-user-unblocked", e =>
                        {
                            e.ConfigureConsumer<UserUnblockedEventConsumer>(context);
                        });

                        // Lệnh này cực kỳ quan trọng: Nó tự động rà soát các Consumer bạn đã đăng ký
                        // và tự động tạo Queue trên RabbitMQ, sau đó liên kết (Bind) Queue đó với Exchange!
                        cfg.ConfigureEndpoints(context);
                    });
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
            // builder.Services.AddAutoMapper(cfg =>
            // {
            //     cfg.AddMaps(typeof(ChatMappingProfile).Assembly);
            // });

            builder.Services.AddGrpcClient<FriendshipGrpcService.FriendshipGrpcServiceClient>(options =>
            {
                // 👉 ĐỌC TỪ APPSETTINGS.JSON
                options.Address = new Uri(builder.Configuration["GrpcUrls:UserService"]!);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return handler;
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
            builder.Services.AddHealthChecks();
        }
    }
}