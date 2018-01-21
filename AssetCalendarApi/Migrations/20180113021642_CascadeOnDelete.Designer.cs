﻿// <auto-generated />
using AssetCalendarApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace AssetCalendarApi.Migrations
{
    [DbContext(typeof(AssetCalendarDbContext))]
    [Migration("20180113021642_CascadeOnDelete")]
    partial class CascadeOnDelete
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AssetCalendarApi.Models.DayJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime");

                    b.Property<Guid>("IdJob");

                    b.HasKey("Id");

                    b.HasIndex("IdJob");

                    b.HasIndex("Date", "IdJob")
                        .IsUnique()
                        .HasName("IX_Date_IdJob");

                    b.ToTable("DaysJobs");
                });

            modelBuilder.Entity("AssetCalendarApi.Models.DayJobWorker", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("IdDayJob");

                    b.Property<Guid>("IdWorker");

                    b.HasKey("Id");

                    b.HasIndex("IdDayJob");

                    b.HasIndex("IdWorker");

                    b.ToTable("DaysJobsWorkers");
                });

            modelBuilder.Entity("AssetCalendarApi.Models.Job", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<int>("Number");

                    b.Property<string>("Type")
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("AssetCalendarApi.Models.Worker", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("Email")
                        .HasMaxLength(200)
                        .IsUnicode(false);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("LastName")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("Phone")
                        .HasMaxLength(10)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("AssetCalendarApi.Models.DayJob", b =>
                {
                    b.HasOne("AssetCalendarApi.Models.Job", "Job")
                        .WithMany("DaysJobs")
                        .HasForeignKey("IdJob")
                        .HasConstraintName("FK_DaysJobs_Job")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AssetCalendarApi.Models.DayJobWorker", b =>
                {
                    b.HasOne("AssetCalendarApi.Models.DayJob", "DayJob")
                        .WithMany("DayJobWorkers")
                        .HasForeignKey("IdDayJob")
                        .HasConstraintName("FK_DaysJobsWorkers_DaysJobs")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AssetCalendarApi.Models.Worker", "Worker")
                        .WithMany("DayJobWorkers")
                        .HasForeignKey("IdWorker")
                        .HasConstraintName("FK_DaysJobsWorkers_Worker")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}