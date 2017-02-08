using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.SqlServer.Migrations
{
    [DbContext(typeof(JobsDbContext))]
    [Migration("20170208074237_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasDefaultSchema("Jobs")
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MR.AspNetCore.Jobs.Models.CronJob", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Cron");

                    b.Property<DateTime>("LastRun");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("TypeName");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("CronJobs");
                });

            modelBuilder.Entity("MR.AspNetCore.Jobs.Models.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Added");

                    b.Property<string>("Data");

                    b.Property<DateTime?>("Due");

                    b.Property<DateTime?>("ExpiresAt");

                    b.Property<int>("Retries");

                    b.Property<string>("StateName")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("StateName");

                    b.HasIndex("Due", "StateName");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("MR.AspNetCore.Jobs.Models.JobQueue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("JobId");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("JobQueue");
                });

            modelBuilder.Entity("MR.AspNetCore.Jobs.Models.JobQueue", b =>
                {
                    b.HasOne("MR.AspNetCore.Jobs.Models.Job", "Job")
                        .WithMany()
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
