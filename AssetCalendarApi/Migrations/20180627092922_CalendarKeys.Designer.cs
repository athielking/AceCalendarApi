﻿// <auto-generated />
using System;
using AssetCalendarApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AssetCalendarApi.Migrations
{
    [DbContext(typeof(AssetCalendarDbContext))]
    [Migration("20180627092922_CalendarKeys")]
    partial class CalendarKeys
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rtm-30799")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AssetCalendarApi.Data.Models.AvailableWorkers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("AvailableWorkers");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Calendar", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("CalendarName")
                        .HasMaxLength(50);

                    b.Property<bool>("Inactive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValueSql("0");

                    b.Property<Guid>("OrganizationId");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Calendars");
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

                    b.Property<Guid>("CalendarId");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("Notes");

                    b.Property<string>("Number");

                    b.HasKey("Id");

                    b.HasIndex("CalendarId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.JobsByDate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Name");

                    b.Property<string>("Notes");

                    b.Property<string>("Number");

                    b.HasKey("Id");

                    b.ToTable("JobsByDate");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.JobsByDateWorker", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<DateTime>("Date");

                    b.Property<Guid>("IdWorker");

                    b.Property<string>("Name");

                    b.Property<string>("Notes");

                    b.Property<string>("Number");

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

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .IsUnicode(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .IsUnicode(false);

                    b.Property<string>("Stripe_CustomerId")
                        .IsRequired()
                        .HasMaxLength(18)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("Organization");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Tag", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<Guid>("CalendarId");

                    b.Property<string>("Color")
                        .HasMaxLength(10);

                    b.Property<string>("Description")
                        .HasMaxLength(25);

                    b.Property<string>("Icon")
                        .HasMaxLength(50);

                    b.Property<int>("TagType");

                    b.HasKey("Id");

                    b.HasIndex("CalendarId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.TagsByJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<string>("Color");

                    b.Property<string>("Description");

                    b.Property<bool>("FromJobDay");

                    b.Property<string>("Icon");

                    b.Property<Guid>("IdJob");

                    b.HasKey("Id");

                    b.ToTable("TagsByJob");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.TagsByJobDate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<string>("Color");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Description");

                    b.Property<bool>("FromJobDay");

                    b.Property<string>("Icon");

                    b.Property<Guid>("IdJob");

                    b.HasKey("Id");

                    b.ToTable("TagsByJobDate");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.TimeOffWorkers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("TimeOffWorkers");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Worker", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<Guid>("CalendarId");

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
                        .HasMaxLength(14)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("CalendarId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.WorkersByJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<Guid>("IdJob");

                    b.Property<string>("LastName");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("WorkersByJob");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.WorkersByJobDate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CalendarId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<Guid>("IdJob");

                    b.Property<string>("LastName");

                    b.Property<string>("Phone");

                    b.HasKey("Id");

                    b.ToTable("WorkersByJobDate");
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.WorkerTags", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<Guid>("IdTag");

                    b.Property<Guid>("IdWorker");

                    b.HasKey("Id");

                    b.HasIndex("IdTag");

                    b.HasIndex("IdWorker");

                    b.ToTable("WorkerTags");
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
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Calendar", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Organization", "Organization")
                        .WithMany("Calendars")
                        .HasForeignKey("OrganizationId")
                        .HasConstraintName("FK_Calendars_Organization")
                        .OnDelete(DeleteBehavior.Restrict);
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
                    b.HasOne("AssetCalendarApi.Data.Models.Calendar", "Calendar")
                        .WithMany("Jobs")
                        .HasForeignKey("CalendarId")
                        .HasConstraintName("FK_Jobs_Calendar")
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
                    b.HasOne("AssetCalendarApi.Data.Models.Calendar", "Calendar")
                        .WithMany("Tags")
                        .HasForeignKey("CalendarId")
                        .HasConstraintName("FK_Tags_Calendar")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.Worker", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Calendar", "Calendar")
                        .WithMany("Workers")
                        .HasForeignKey("CalendarId")
                        .HasConstraintName("FK_Workers_Calendar")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("AssetCalendarApi.Data.Models.WorkerTags", b =>
                {
                    b.HasOne("AssetCalendarApi.Data.Models.Tag", "Tag")
                        .WithMany("WorkerTags")
                        .HasForeignKey("IdTag")
                        .HasConstraintName("FK_WorkerTags_Tag")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AssetCalendarApi.Data.Models.Worker", "Worker")
                        .WithMany("WorkerTags")
                        .HasForeignKey("IdWorker")
                        .HasConstraintName("FK_WorkerTags_Worker")
                        .OnDelete(DeleteBehavior.Cascade);
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
