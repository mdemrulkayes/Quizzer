using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                schema: "Identity",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserId",
                schema: "Identity",
                table: "UserTokens");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                schema: "Identity",
                table: "UserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("3123befc-4fd0-4493-b28e-46c1ed881ca4"),
                column: "ConcurrencyStamp",
                value: "STATIC-ADMIN-STAMP");

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9d476df0-1663-43af-b06b-af945b07db45"),
                column: "ConcurrencyStamp",
                value: "STATIC-ADMIN-STAMP");

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("ac3c30c4-fcbd-4e5a-ab8b-8f6179a65120"),
                column: "ConcurrencyStamp",
                value: "STATIC-ADMIN-STAMP");

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f0cc1d90-471c-4563-b20a-12acdb47735b"),
                column: "ConcurrencyStamp",
                value: "STATIC-ADMIN-STAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                schema: "Identity",
                table: "UserTokens");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                schema: "Identity",
                table: "UserTokens",
                columns: new[] { "LoginProvider", "UserId", "Name" });

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("3123befc-4fd0-4493-b28e-46c1ed881ca4"),
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9d476df0-1663-43af-b06b-af945b07db45"),
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("ac3c30c4-fcbd-4e5a-ab8b-8f6179a65120"),
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f0cc1d90-471c-4563-b20a-12acdb47735b"),
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                schema: "Identity",
                table: "UserTokens",
                column: "UserId");
        }
    }
}
