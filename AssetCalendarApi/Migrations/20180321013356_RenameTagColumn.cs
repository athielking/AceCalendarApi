using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Migrations
{
    public partial class RenameTagColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatIcon",
                table: "Tags");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Tags",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Tags");

            migrationBuilder.AddColumn<string>(
                name: "MatIcon",
                table: "Tags",
                nullable: true);
        }
    }
}
