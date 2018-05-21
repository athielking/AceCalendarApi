using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Migrations
{
    public partial class WorkerTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TagType",
                table: "Tags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WorkerTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTag = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdWorker = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkerTags_Tag",
                        column: x => x.IdTag,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkerTags_Worker",
                        column: x => x.IdWorker,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkerTags_IdTag",
                table: "WorkerTags",
                column: "IdTag");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerTags_IdWorker",
                table: "WorkerTags",
                column: "IdWorker");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerTags");

            migrationBuilder.DropColumn(
                name: "TagType",
                table: "Tags");
        }
    }
}
