using ChatApp.Shared.Behaviors;
using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Middlewares;
using ChatApp.Shared.Repositories;
using ChatApp.Shared.Services;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.DatabaseContext;

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

            // Đăng ký FluentValidation
            builder.Services.AddValidatorsFromAssembly(applicationAssembly);
        }

        private static void RegisterInfrastructure(WebApplicationBuilder builder)
        {
            // Setup PostgreSQL
            builder.Services.AddDbContextPool<UserDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<UserDbContext>());
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Setup MassTransit (RabbitMQ)
            builder.Services.AddMassTransit(x =>
            {
                // TODO: Chút nữa chúng ta sẽ cấu hình Consumer ở đây

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

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }
    }
}