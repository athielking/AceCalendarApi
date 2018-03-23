using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Migrations
{
    public partial class DayJobTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DaysJobsTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdDayJob = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTag = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysJobsTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayJobTag_DayJob",
                        column: x => x.IdDayJob,
                        principalTable: "DaysJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayJobTag_Tag",
                        column: x => x.IdTag,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DaysJobsTags_IdDayJob",
                table: "DaysJobsTags",
                column: "IdDayJob");

            migrationBuilder.CreateIndex(
                name: "IX_DaysJobsTags_IdTag",
                table: "DaysJobsTags",
                column: "IdTag");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DaysJobsTags");
        }
    }
}
