using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Migrations
{
    public partial class CascadeOnDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DaysJobs_Job",
                table: "DaysJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_DaysJobsWorkers_DaysJobs",
                table: "DaysJobsWorkers");

            migrationBuilder.DropForeignKey(
                name: "FK_DaysJobsWorkers_Worker",
                table: "DaysJobsWorkers");

            migrationBuilder.AddForeignKey(
                name: "FK_DaysJobs_Job",
                table: "DaysJobs",
                column: "IdJob",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DaysJobsWorkers_DaysJobs",
                table: "DaysJobsWorkers",
                column: "IdDayJob",
                principalTable: "DaysJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DaysJobsWorkers_Worker",
                table: "DaysJobsWorkers",
                column: "IdWorker",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DaysJobs_Job",
                table: "DaysJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_DaysJobsWorkers_DaysJobs",
                table: "DaysJobsWorkers");

            migrationBuilder.DropForeignKey(
                name: "FK_DaysJobsWorkers_Worker",
                table: "DaysJobsWorkers");

            migrationBuilder.AddForeignKey(
                name: "FK_DaysJobs_Job",
                table: "DaysJobs",
                column: "IdJob",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DaysJobsWorkers_DaysJobs",
                table: "DaysJobsWorkers",
                column: "IdDayJob",
                principalTable: "DaysJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DaysJobsWorkers_Worker",
                table: "DaysJobsWorkers",
                column: "IdWorker",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
