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
    [Migration("20180319214926_DayJobTags")]
    partial class DayJobTags
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AssetCalendarApi.Data.Models.AvailableWorkers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<Guid>("OrganizationId");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("AvailableWorkers");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.CalendarUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<Guid>("OrganizationId");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("OrganizationId");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayJob", b =>
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

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayJobTag", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<Guid>("IdDayJob");

                    b.Property<Guid>("IdTag");

                    b.HasKey("Id");

                    b.HasIndex("IdDayJob");

                    b.HasIndex("IdTag");

                    b.ToTable("DaysJobsTags");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayJobWorker", b =>
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

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayOffWorker", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime");

                    b.Property<Guid>("IdWorker");

                    b.HasKey("Id");

                    b.HasIndex("IdWorker");

                    b.ToTable("DayOffWorker");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Job", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("Notes");

                    b.Property<string>("Number");

                    b.Property<Guid>("OrganizationId");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.JobsByDate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Name");

                    b.Property<string>("Notes");

                    b.Property<string>("Number");

                    b.Property<Guid>("OrganizationId");

                    b.HasKey("Id");

                    b.ToTable("JobsByDate");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.JobsByDateWorker", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<Guid>("IdWorker");

                    b.Property<string>("Name");

                    b.Property<string>("Notes");

                    b.Property<string>("Number");

                    b.Property<Guid>("OrganizationId");

                    b.HasKey("Id");

                    b.ToTable("JobsByDateWorker");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.JobTags", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<Guid>("IdJob");

                    b.Property<Guid>("IdTag");

                    b.HasKey("Id");

                    b.HasIndex("IdJob");

                    b.HasIndex("IdTag");

                    b.ToTable("JobTags");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Organization", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("Organization");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Tag", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("Color")
                        .HasMaxLength(10);

                    b.Property<string>("Description")
                        .HasMaxLength(25);

                    b.Property<string>("MatIcon");

                    b.Property<Guid>("OrganizationId");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.TagsByJobDate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Color");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Description");

                    b.Property<Guid>("IdJob");

                    b.Property<string>("MatIcon");

                    b.Property<Guid>("OrganizationId");

                    b.HasKey("Id");

                    b.ToTable("TagsByJobDate");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.TimeOffWorkers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<Guid>("OrganizationId");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("TimeOffWorkers");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Worker", b =>
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

                    b.Property<Guid>("OrganizationId");

                    b.Property<string>("Phone")
                        .HasMaxLength(14)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.WorkersByJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<Guid>("IdJob");

                    b.Property<string>("LastName");

                    b.Property<Guid>("OrganizationId");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("WorkersByJob");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.WorkersByJobDate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<Guid>("IdJob");

                    b.Property<string>("LastName");

                    b.Property<Guid>("OrganizationId");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("WorkersByJobDate");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.CalendarUser", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Organization", "Organization")
                        .WithMany("CalendarUsers")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayJob", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Job", "Job")
                        .WithMany("DaysJobs")
                        .HasForeignKey("IdJob")
                        .HasConstraintName("FK_DaysJobs_Job")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayJobTag", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.DayJob", "DayJob")
                        .WithMany("DayJobTags")
                        .HasForeignKey("IdDayJob")
                        .HasConstraintName("FK_DayJobTag_DayJob")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AssetCalendarApi.Data.Models.Tag", "Tag")
                        .WithMany("DayJobTags")
                        .HasForeignKey("IdTag")
                        .HasConstraintName("FK_DayJobTag_Tag")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayJobWorker", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.DayJob", "DayJob")
                        .WithMany("DayJobWorkers")
                        .HasForeignKey("IdDayJob")
                        .HasConstraintName("FK_DaysJobsWorkers_DaysJobs")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AssetCalendarApi.Data.Models.Worker", "Worker")
                        .WithMany("DayJobWorkers")
                        .HasForeignKey("IdWorker")
                        .HasConstraintName("FK_DaysJobsWorkers_Worker")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.DayOffWorker", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Worker", "Worker")
                        .WithMany("DayOffWorkers")
                        .HasForeignKey("IdWorker")
                        .HasConstraintName("FK_DayOffWorkers_Worker")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Job", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Organization", "Organization")
                        .WithMany("Jobs")
                        .HasForeignKey("OrganizationId")
                        .HasConstraintName("FK_Jobs_Organization")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.JobTags", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Job", "Job")
                        .WithMany("Tags")
                        .HasForeignKey("IdJob")
                        .HasConstraintName("FK_JobTags_Job")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AssetCalendarApi.Data.Models.Tag", "Tag")
                        .WithMany("JobTags")
                        .HasForeignKey("IdTag")
                        .HasConstraintName("FK_JobTags_Tag")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Tag", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Organization", "Organization")
                        .WithMany("Tags")
                        .HasForeignKey("OrganizationId")
                        .HasConstraintName("FK_Tags_Organization")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Worker", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Organization", "Organization")
                        .WithMany("Workers")
                        .HasForeignKey("OrganizationId")
                        .HasConstraintName("FK_Workers_Organization")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.CalendarUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.CalendarUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AssetCalendarApi.Data.Models.CalendarUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.CalendarUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
