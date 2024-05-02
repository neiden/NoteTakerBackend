using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TokenTest.Migrations
{
    /// <inheritdoc />
    public partial class AddNewStudentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
