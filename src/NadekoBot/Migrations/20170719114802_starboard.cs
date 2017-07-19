using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NadekoBot.Migrations
{
    public partial class starboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StarGuilds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<ulong>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: true),
                    GuildId = table.Column<ulong>(nullable: false),
                    Required = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarGuilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StarMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateAdded = table.Column<DateTime>(nullable: true),
                    GuildId = table.Column<int>(nullable: false),
                    MessageId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarMessages_StarGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "StarGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StarMessages_GuildId",
                table: "StarMessages",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarMessages");

            migrationBuilder.DropTable(
                name: "StarGuilds");
        }
    }
}
