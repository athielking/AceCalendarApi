using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using AssetCalendarApi.Tools;

namespace AssetCalendarApi.Data
{
    public partial class AssetCalendarDbContext : IdentityDbContext<AceUser>
    {
        #region Data Members

        private IConfiguration _configuration;

        private IHostingEnvironment _environment;

        #endregion

        #region Properties

        public DbSet<DayJob> DaysJobs { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<DayJobWorker> DaysJobsWorkers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<DayOffWorker> DayOffWorkers { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<JobTags> JobTags { get; set; }
        public DbSet<WorkerTags> WorkerTags { get; set; }
        public DbSet<DayJobTag> DaysJobsTags { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<CalendarUser> CalendarUsers { get; set; }

        //Views
        public DbSet<JobsByDate> JobsByDate { get; set; }
        public DbSet<JobsByDateWorker> JobsByDateWorker { get; set; }
        public DbSet<AvailableWorkers> AvailableWorkers { get; set; }
        public DbSet<TimeOffWorkers> TimeOffWorkers { get; set; }
        public DbSet<WorkersByJob> WorkersByJob { get; set; }
        public DbSet<WorkersByJobDate> WorkersByJobDate { get; set; }
        public DbSet<TagsByJob> TagsByJob { get; set; }
        public DbSet<TagsByJobDate> TagsByJobDate { get; set; }

        #endregion

        #region Constructor

        public AssetCalendarDbContext(IConfiguration configuration, IHostingEnvironment environment, DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
            _configuration = configuration;
            _environment = environment;
        }

        #endregion

        #region Overrides

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (_environment.IsProduction())
                    optionsBuilder.UseSqlServer(_configuration.GetConnectionString("AceCalendarDb_Prod"));
                else
                    optionsBuilder.UseSqlServer(_configuration.GetConnectionString("AceCalendarDb_Stg"));
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

                entity.HasOne(d => d.Calendar)
                     .WithMany(j => j.Jobs)
                     .HasForeignKey(d => d.CalendarId)
                     .OnDelete(DeleteBehavior.Restrict)
                     .HasConstraintName("FK_Jobs_Calendar");
            });

            modelBuilder.Entity<JobTags>(entity =>
            {
                entity.ToTable("JobTags");
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(t => t.Job)
                    .WithMany(j => j.Tags)
                    .HasForeignKey(j => j.IdJob)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_JobTags_Job");

                entity.HasOne(t => t.Tag)
                    .WithMany(t => t.JobTags)
                    .HasForeignKey(t => t.IdTag)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_JobTags_Tag"); 
            });

            modelBuilder.Entity<WorkerTags>(entity =>
            {
                entity.ToTable("WorkerTags");
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(t => t.Worker)
                    .WithMany(j => j.WorkerTags)
                    .HasForeignKey(j => j.IdWorker)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_WorkerTags_Worker");

                entity.HasOne(t => t.Tag)
                    .WithMany(t => t.WorkerTags)
                    .HasForeignKey(t => t.IdTag)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_WorkerTags_Tag");
            });

            modelBuilder.Entity<DayJobTag>(entity =>
            {
                entity.ToTable("DaysJobsTags");
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(t => t.DayJob)
                    .WithMany(j => j.DayJobTags)
                    .HasForeignKey(j => j.IdDayJob)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DayJobTag_DayJob");

                entity.HasOne(t => t.Tag)
                    .WithMany(t => t.DayJobTags)
                    .HasForeignKey(t => t.IdTag)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DayJobTag_Tag");
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

                entity.HasOne(d => d.Calendar)
                     .WithMany(j => j.Workers)
                     .HasForeignKey(d => d.CalendarId)
                     .OnDelete(DeleteBehavior.Restrict)
                     .HasConstraintName("FK_Workers_Calendar");
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organization");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(200)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Stripe_CustomerId)
                    .HasMaxLength(18)
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

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tags");
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Description).HasMaxLength(25);
                entity.Property(e => e.Color).HasMaxLength(10);
                entity.Property(e => e.Icon).HasMaxLength(50);

                entity.HasOne(d => d.Calendar)
                     .WithMany(t => t.Tags)
                     .HasForeignKey(d => d.CalendarId)
                     .OnDelete(DeleteBehavior.Restrict)
                     .HasConstraintName("FK_Tags_Calendar");
            });

            modelBuilder.Entity<Calendar>(entity =>
            {
               entity.ToTable("Calendars");
               entity.Property(e => e.Id).ValueGeneratedNever();
               entity.Property(e => e.CalendarName).HasMaxLength(50);
               entity.Property(e => e.Inactive)
                   .HasColumnType("bit")
                   .HasDefaultValueSql("0");

                entity.HasOne(c => c.Organization)
                    .WithMany(o => o.Calendars)
                    .HasForeignKey(c => c.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Calendars_Organization");
            });

            modelBuilder.Entity<CalendarUser>(entity =>
           {
               entity.ToTable("CalendarUsers");
               entity.Property(e => e.Id).ValueGeneratedNever();

               entity.HasOne(c => c.Calendar)
                   .WithMany(c => c.CalendarUsers)
                   .HasForeignKey(c => c.CalendarId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_CalendarUsers_Calendar");
           });

            //modelBuilder.Entity<JobsByDate>(entity =>
            //{
            //    entity.Property(e => e.Date).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<JobsByDateWorker>(entity =>
            //{
            //    entity.Property(e => e.Date).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<AvailableWorkers>(entity =>
            //{
            //    entity.Property(e => e.Date).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<TimeOffWorkers>(entity =>
            //{
            //    entity.Property(e => e.Date).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<WorkersByJob>(entity => { });

            //modelBuilder.Entity<WorkersByJobDate>(entity =>
            //{
            //    entity.Property(e => e.Date).HasColumnType("datetime");
            //});

        }

        #endregion
    }
}
