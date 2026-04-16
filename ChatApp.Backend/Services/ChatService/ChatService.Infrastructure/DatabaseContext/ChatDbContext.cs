using System.Reflection;
using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.DatabaseContext
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Participant> Participants { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<Conversation>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Participant>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        }
    }
}