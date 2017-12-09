using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MR.AspNetCore.Jobs.Migrations
{
    public partial class add_JobQueueFetchedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                schema: "Jobs",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FetchedAt",
                schema: "Jobs",
                table: "JobQueue",
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

            migrationBuilder.CreateIndex(
                name: "IX_JobQueue_FetchedAt",
                schema: "Jobs",
                table: "JobQueue",
                column: "FetchedAt");
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

            migrationBuilder.DropIndex(
                name: "IX_JobQueue_FetchedAt",
                schema: "Jobs",
                table: "JobQueue");

            migrationBuilder.DropColumn(
                name: "Updated",
                schema: "Jobs",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "FetchedAt",
                schema: "Jobs",
                table: "JobQueue");
        }
    }
}
