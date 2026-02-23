using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PermissionAuth.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id          = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name        = table.Column<string>(nullable: false),
                    Module      = table.Column<string>(nullable: false),
                    Action      = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CreatedAt   = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Permissions", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id        = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name      = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Roles", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id           = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Username     = table.Column<string>(nullable: false),
                    Email        = table.Column<string>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    CreatedAt    = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId       = table.Column<int>(nullable: false),
                    PermissionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey("FK_RolePermissions_Roles_RoleId", x => x.RoleId, "Roles", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_RolePermissions_Permissions_PermissionId", x => x.PermissionId, "Permissions", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey("FK_UserRoles_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserRoles_Roles_RoleId", x => x.RoleId, "Roles", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Permissions_Name", "Permissions", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_Roles_Name", "Roles", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("UserRoles");
            migrationBuilder.DropTable("RolePermissions");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("Roles");
            migrationBuilder.DropTable("Permissions");
        }
    }
}
