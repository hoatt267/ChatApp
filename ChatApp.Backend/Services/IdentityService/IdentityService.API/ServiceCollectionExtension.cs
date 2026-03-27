using System.Text;
using FluentValidation;
using IdentityService.API.Middlewares;
using IdentityService.Application.Behaviors;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Authentication;
using IdentityService.Infrastructure.DatabaseContext;
using IdentityService.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.API
{
    public static class ServiceCollectionExtension
    {
        public static void RegisterExtension(this WebApplicationBuilder builder)
        {
            RegisterInfrastructure(builder);
        }

        private static void RegisterInfrastructure(WebApplicationBuilder builder)
        {
            // Setup Database
            builder.Services.AddDbContextPool<IdentityDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            // Cấu hình CORS mở cửa cho Frontend
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

            builder.Services.AddHealthChecks();
        }
    }
}