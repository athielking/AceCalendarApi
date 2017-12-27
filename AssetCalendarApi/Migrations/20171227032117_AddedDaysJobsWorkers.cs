using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Migrations
{
    public partial class AddedDaysJobsWorkers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DaysJobs",
                table: "DaysJobs");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "DaysJobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DaysJobs",
                table: "DaysJobs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DaysJobsWorkers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdDayJob = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdWorker = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysJobsWorkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaysJobsWorkers_DaysJobs",
                        column: x => x.IdDayJob,
                        principalTable: "DaysJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DaysJobsWorkers_Worker",
                        column: x => x.IdWorker,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DaysJobsWorkers_IdDayJob",
                table: "DaysJobsWorkers",
                column: "IdDayJob");

            migrationBuilder.CreateIndex(
                name: "IX_DaysJobsWorkers_IdWorker",
                table: "DaysJobsWorkers",
                column: "IdWorker");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DaysJobsWorkers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DaysJobs",
                table: "DaysJobs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DaysJobs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DaysJobs",
                table: "DaysJobs",
                columns: new[] { "Date", "IdJob" });
        }
    }
}
