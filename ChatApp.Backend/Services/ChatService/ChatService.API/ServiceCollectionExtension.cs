using ChatService.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

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
            // Setup Database PostgreSQL cho Chat Service
            builder.Services.AddDbContextPool<ChatDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Tương lai sẽ add thêm CORS, SignalR, Repository, MediatR ở đây...
        }
    }
}