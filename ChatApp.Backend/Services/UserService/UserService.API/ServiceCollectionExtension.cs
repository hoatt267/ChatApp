using System.Text;
using ChatApp.Shared.Behaviors;
using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Middlewares;
using ChatApp.Shared.Repositories;
using ChatApp.Shared.Services;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.EventConsumers;
using UserService.Application.Interfaces;
using UserService.Infrastructure.DatabaseContext;
using UserService.Infrastructure.Repositories;

namespace UserService.API
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
            builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

            // Lấy Assembly của Application layer (sẽ dùng để scan MediatR, Validator sau này)
            var applicationAssembly = typeof(UserService.Application.AssemblyReference).Assembly;


            // Đăng ký MediatR & Pipeline Behavior
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // Đăng ký AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(applicationAssembly));

            // Đăng ký FluentValidation
            builder.Services.AddValidatorsFromAssembly(applicationAssembly);

            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        }

        private static void RegisterInfrastructure(WebApplicationBuilder builder)
        {
            // Setup PostgreSQL
            builder.Services.AddDbContextPool<UserDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<UserDbContext>());
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Đăng ký Authentication & JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
                    };
                });

            builder.Services.AddAuthorization();

            // Setup MassTransit (RabbitMQ)
            builder.Services.AddMassTransit(x =>
            {
                // 1. Khai báo Consumer
                x.AddConsumer<UserCreatedEventConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("user-service-user-created", e =>
                    {
                        e.ConfigureConsumer<UserCreatedEventConsumer>(context);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddGrpc();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }
    }
}