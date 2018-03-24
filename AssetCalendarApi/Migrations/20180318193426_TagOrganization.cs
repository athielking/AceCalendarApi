using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Migrations
{
    public partial class TagOrganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Tags",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OrganizationId",
                table: "Tags",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Organization",
                table: "Tags",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Organization",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_OrganizationId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Tags");
        }
    }
}
