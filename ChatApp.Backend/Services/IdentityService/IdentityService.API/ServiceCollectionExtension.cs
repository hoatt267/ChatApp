using IdentityService.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
        }
    }
}