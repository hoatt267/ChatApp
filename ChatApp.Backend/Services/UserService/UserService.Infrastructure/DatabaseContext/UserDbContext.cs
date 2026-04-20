using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.DatabaseContext
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<Friendship> Friendships { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<UserProfile>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Friendship>().HasQueryFilter(f => !f.IsDeleted);

            // CẬP NHẬT INDEX CHỐNG TRÙNG LẶP CHO FRIENDSHIP
            // Đảm bảo rằng chỉ những record chưa bị xóa (IsDeleted = false) mới bị ràng buộc Unique.
            // Điều này cho phép 2 người đã hủy kết bạn (IsDeleted = true) có thể gửi lại lời mời.
            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.RequesterId, f.ReceiverId })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            // ==========================================
            // 3. CẤU HÌNH TÌM KIẾM (FUZZY SEARCH INDEX)
            // ==========================================

            // Bật Extension Trigram của PostgreSQL
            modelBuilder.HasPostgresExtension("pg_trgm");
            // Bật Extension Unaccent để loại bỏ dấu khi tìm kiếm
            modelBuilder.HasPostgresExtension("unaccent");

            // Đánh GIN Index cho cột FullName
            modelBuilder.Entity<UserProfile>()
                .HasIndex(p => p.FullName)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops"); // Khai báo dùng toán tử Trigram
        }
    }
}