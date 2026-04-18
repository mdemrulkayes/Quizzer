using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUserProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLoginTime",
                schema: "Identity",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginTime",
                schema: "Identity",
                table: "Users");
        }
    }
}
