using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Migrations
{
    public partial class AddedJobTypeColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Jobs",
                type: "varchar(25)",
                unicode: false,
                maxLength: 25,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Date_IdJob",
                table: "DaysJobs",
                columns: new[] { "Date", "IdJob" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Date_IdJob",
                table: "DaysJobs");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Jobs");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Jobs",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
