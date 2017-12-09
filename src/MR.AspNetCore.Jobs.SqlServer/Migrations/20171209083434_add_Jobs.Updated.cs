using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MR.AspNetCore.Jobs.SqlServer.Migrations
{
    public partial class add_JobsUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                schema: "Jobs",
                table: "Jobs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Added",
                schema: "Jobs",
                table: "Jobs",
                column: "Added");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Updated",
                schema: "Jobs",
                table: "Jobs",
                column: "Updated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_Added",
                schema: "Jobs",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Updated",
                schema: "Jobs",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Updated",
                schema: "Jobs",
                table: "Jobs");
        }
    }
}
