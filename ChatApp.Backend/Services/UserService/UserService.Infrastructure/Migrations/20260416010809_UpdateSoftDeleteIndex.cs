using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSoftDeleteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Friendships_RequesterId_ReceiverId",
                table: "Friendships");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_ReceiverId",
                table: "Friendships",
                columns: new[] { "RequesterId", "ReceiverId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Friendships_RequesterId_ReceiverId",
                table: "Friendships");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_ReceiverId",
                table: "Friendships",
                columns: new[] { "RequesterId", "ReceiverId" },
                unique: true);
        }
    }
}
