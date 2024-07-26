using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TokenTest.Migrations
{
    /// <inheritdoc />
    public partial class FixedUserConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Login_Email",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_Login",
                table: "User",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_Login",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_Login_Email",
                table: "User",
                columns: new[] { "Login", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL");
        }
    }
}
