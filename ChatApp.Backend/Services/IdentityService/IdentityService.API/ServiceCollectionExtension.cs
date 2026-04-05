using System.Text;
using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Repositories;
using FluentValidation;
using ChatApp.Shared.Middlewares;
using ChatApp.Shared.Behaviors;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Authentication;
using IdentityService.Infrastructure.DatabaseContext;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MassTransit;
using ChatApp.Shared.Services;

namespace IdentityService.API
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
            // Đăng ký Blob Storage Service
            builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        }

        private static void RegisterInfrastructure(WebApplicationBuilder builder)
        {
            // Setup Database
            builder.Services.AddDbContextPool<IdentityDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký DbContext để có thể inject vào Repository
            builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<IdentityDbContext>());

            // Register generic repository for application handlers.
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // 1. Lấy Assembly của tầng Application (Dùng chung cho cả Mapper, MediatR, FluentValidation)
            var applicationAssembly = typeof(IdentityService.Application.Mappings.MappingProfile).Assembly;

            // 2. Đăng ký AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<IdentityService.Application.Mappings.MappingProfile>();
            });

            // 3. Đăng ký MediatR và Pipeline Behavior
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // Register RabbitMQ
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            //Register JWT
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // Bỏ độ trễ 5 phút mặc định của .NET
                };
            });
            builder.Services.AddAuthorization();

            // 4. Đăng ký FluentValidation
            builder.Services.AddValidatorsFromAssembly(applicationAssembly);

            //Regiser Global Exception Handler
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            builder.Services.AddScoped<IJwtProvider, JwtProvider>();

            builder.Services.AddHealthChecks();
        }
    }
}