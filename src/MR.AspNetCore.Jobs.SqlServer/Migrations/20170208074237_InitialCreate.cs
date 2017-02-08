using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MR.AspNetCore.Jobs.SqlServer.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
				name: "Jobs");

			migrationBuilder.CreateTable(
				name: "CronJobs",
				schema: "Jobs",
				columns: table => new
				{
					Id = table.Column<string>(nullable: false),
					Cron = table.Column<string>(nullable: true),
					LastRun = table.Column<DateTime>(nullable: false),
					Name = table.Column<string>(nullable: false),
					TypeName = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_CronJobs", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Jobs",
				schema: "Jobs",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					Added = table.Column<DateTime>(nullable: false),
					Data = table.Column<string>(nullable: true),
					Due = table.Column<DateTime>(nullable: true),
					ExpiresAt = table.Column<DateTime>(nullable: true),
					Retries = table.Column<int>(nullable: false),
					StateName = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Jobs", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "JobQueue",
				schema: "Jobs",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					JobId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_JobQueue", x => x.Id);
					table.ForeignKey(
						name: "FK_JobQueue_Jobs_JobId",
						column: x => x.JobId,
						principalSchema: "Jobs",
						principalTable: "Jobs",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_CronJobs_Name",
				schema: "Jobs",
				table: "CronJobs",
				column: "Name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Jobs_StateName",
				schema: "Jobs",
				table: "Jobs",
				column: "StateName");

			migrationBuilder.CreateIndex(
				name: "IX_Jobs_Due_StateName",
				schema: "Jobs",
				table: "Jobs",
				columns: new[] { "Due", "StateName" });

			migrationBuilder.CreateIndex(
				name: "IX_JobQueue_JobId",
				schema: "Jobs",
				table: "JobQueue",
				column: "JobId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "CronJobs",
				schema: "Jobs");

			migrationBuilder.DropTable(
				name: "JobQueue",
				schema: "Jobs");

			migrationBuilder.DropTable(
				name: "Jobs",
				schema: "Jobs");
		}
	}
}
