using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AssetCalendarApi.Data
{
    public partial class AssetCalendarDbContext : IdentityDbContext<CalendarUser>
    {
        #region Data Members

        private IConfiguration _configuration;

        #endregion

        #region Properties

        public DbSet<DayJob> DaysJobs { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<DayJobWorker> DaysJobsWorkers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<DayOffWorker> DayOffWorkers { get; set; }

        //Views
        public DbSet<JobsByDate> JobsByDate { get; set; }
        public DbSet<JobsByDateWorker> JobsByDateWorker { get; set; }
        public DbSet<AvailableWorkers> AvailableWorkers { get; set; }
        public DbSet<TimeOffWorkers> TimeOffWorkers { get; set; }
        public DbSet<WorkersByJob> WorkersByJob { get; set; }
        public DbSet<WorkersByJobDate> WorkersByJobDate { get; set; }
        #endregion

        #region Constructor

        public AssetCalendarDbContext(IConfiguration configuration, DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
            _configuration = configuration;
        }

        #endregion

        #region Overrides

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var server = "snare.arvixe.com";
                var db = "AssetCalendarDb";
                var pw = "d70wIakr5oxP";
                var user = "acecalendar";


                var connectionString = $"Server={server}; Database={db}; User Id={user}; Password={pw}; MultipleActiveResultSets=true";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DayJobWorker>(entity =>
            {
                entity.ToTable("DaysJobsWorkers");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Worker)
                     .WithMany(w => w.DayJobWorkers)
                     .HasForeignKey(d => d.IdWorker)
                     .OnDelete(DeleteBehavior.Cascade)
                     .HasConstraintName("FK_DaysJobsWorkers_Worker");

                entity.HasOne(d => d.DayJob)
                     .WithMany(j => j.DayJobWorkers)
                     .HasForeignKey(d => d.IdDayJob)
                     .OnDelete(DeleteBehavior.Cascade)
                     .HasConstraintName("FK_DaysJobsWorkers_DaysJobs");

            });

            modelBuilder.Entity<DayJob>(entity =>
            {
                entity.ToTable("DaysJobs");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.DaysJobs)
                    .HasForeignKey(d => d.IdJob)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DaysJobs_Job");

                entity.HasIndex(i => new { i.Date, i.IdJob })
                    .IsUnique(true)
                    .HasName("IX_Date_IdJob");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("Jobs");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Organization)
                     .WithMany(j => j.Jobs)
                     .HasForeignKey(d => d.OrganizationId)
                     .OnDelete(DeleteBehavior.Restrict)
                     .HasConstraintName("FK_Jobs_Organization");

            });

            modelBuilder.Entity<Worker>(entity =>
            {
                entity.ToTable("Workers");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.HasOne(d => d.Organization)
                     .WithMany(j => j.Workers)
                     .HasForeignKey(d => d.OrganizationId)
                     .OnDelete(DeleteBehavior.Restrict)
                     .HasConstraintName("FK_Workers_Organization");
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organization");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired()
                    .IsUnicode(false);

            });

            modelBuilder.Entity<DayOffWorker>(entity =>
            {
                entity.ToTable("DayOffWorker");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.Worker)
                    .WithMany(w => w.DayOffWorkers)
                    .HasForeignKey(d => d.IdWorker)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DayOffWorkers_Worker");
            });

            modelBuilder.Entity<JobsByDate>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<JobsByDateWorker>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<AvailableWorkers>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<TimeOffWorkers>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<WorkersByJob>(entity => { });

            modelBuilder.Entity<WorkersByJobDate>(entity => 
            {
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

        }

        #endregion
    }
}
