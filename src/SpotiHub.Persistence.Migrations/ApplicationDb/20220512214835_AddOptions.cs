using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotiHub.Persistence.Migrations.ApplicationDb
{
    public partial class AddOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultEmoji = table.Column<string>(type: "text", nullable: false),
                    LimitedAvailability = table.Column<bool>(type: "boolean", nullable: false),
                    GenreEmojis = table.Column<bool>(type: "boolean", nullable: false),
                    FallbackMessage = table.Column<string>(type: "text", nullable: true),
                    FallbackEmoji = table.Column<string>(type: "text", nullable: true),
                    ClearAfter = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.ApplicationUserId);
                    table.ForeignKey(
                        name: "FK_Options_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Options");
        }
    }
}
