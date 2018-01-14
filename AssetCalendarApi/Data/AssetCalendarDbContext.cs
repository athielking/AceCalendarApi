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

        public virtual DbSet<DayJob> DaysJobs { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<Worker> Workers { get; set; }
        public virtual DbSet<DayJobWorker> DaysJobsWorkers { get; set; }

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
                optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSqlLocalDB;Database=AssetCalendarDb;Trusted_Connection=True;");
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

                entity.Property(e => e.Type)
                    .HasMaxLength(25)
                    .IsUnicode(false);

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
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });
        } 
        
        #endregion
    }
}
