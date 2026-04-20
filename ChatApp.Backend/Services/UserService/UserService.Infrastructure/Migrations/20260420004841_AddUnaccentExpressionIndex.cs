using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnaccentExpressionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            // --- ÉP HÀM UNACCENT TRỞ THÀNH IMMUTABLE ĐỂ TẠO INDEX ---
            migrationBuilder.Sql("ALTER FUNCTION unaccent(text) IMMUTABLE;");
            // --- THÊM ĐOẠN NÀY ĐỂ TẠO INDEX ---
            migrationBuilder.Sql(@"
                CREATE INDEX idx_user_fullname_unaccent_trgm 
                ON ""UserProfiles"" 
                USING gin (unaccent(""FullName"") gin_trgm_ops);
            ");

            // ------------------------------------------
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_user_fullname_unaccent_trgm;");

            // Trả hàm unaccent về trạng thái STABLE ban đầu nếu Rollback
            migrationBuilder.Sql("ALTER FUNCTION unaccent(text) STABLE;");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
