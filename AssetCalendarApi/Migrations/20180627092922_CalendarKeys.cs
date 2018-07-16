using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AssetCalendarApi.Migrations
{
    public partial class CalendarKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Organization",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Organization",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Organization",
                table: "Workers");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Workers",
                newName: "CalendarId");

            migrationBuilder.RenameIndex(
                name: "IX_Workers_OrganizationId",
                table: "Workers",
                newName: "IX_Workers_CalendarId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Tags",
                newName: "CalendarId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_OrganizationId",
                table: "Tags",
                newName: "IX_Tags_CalendarId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Jobs",
                newName: "CalendarId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_OrganizationId",
                table: "Jobs",
                newName: "IX_Jobs_CalendarId");

            migrationBuilder.CreateTable(
                name: "Calendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CalendarName = table.Column<string>(maxLength: 50, nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendars_Organization",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
               name: "CalendarUsers",
               columns: table => new
               {
                   Id = table.Column<Guid>(nullable: false),
                   CalendarId = table.Column<Guid>(nullable: false),
                   OrganizationId = table.Column<Guid>(nullable: false),
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_CalendarUsers", x => x.Id);
                   table.ForeignKey(
                       name: "FK_CalendarUsers_Calendar",
                       column: x => x.CalendarId,
                       principalTable: "Calendars",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Restrict);
               });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Organization SET Id = Calendars.Id FROM Organization INNER JOIN Calendars ON Organization.Id = Calendars.Id");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Calendar",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Calendar",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Calendar",
                table: "Workers");

            migrationBuilder.DropTable(
                name: "Calendars");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "WorkersByJobDate",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "WorkersByJob",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "Workers",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Workers_CalendarId",
                table: "Workers",
                newName: "IX_Workers_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "TimeOffWorkers",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "TagsByJobDate",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "TagsByJob",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "Tags",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_CalendarId",
                table: "Tags",
                newName: "IX_Tags_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "JobsByDateWorker",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "JobsByDate",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "Jobs",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_CalendarId",
                table: "Jobs",
                newName: "IX_Jobs_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "CalendarId",
                table: "AvailableWorkers",
                newName: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Organization",
                table: "Jobs",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Organization",
                table: "Tags",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Organization",
                table: "Workers",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            DropAndCreateViewsRollback(migrationBuilder);
        }

        private void DropAndCreateViewsUpgrade(MigrationBuilder migrationBuilder)
        {
            #region WorkersNotAvailable

            migrationBuilder.Sql("DROP VIEW [WorkersNotAvailable]");
            migrationBuilder.Sql(@"CREATE VIEW [WorkersNotAvailable]
AS
SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.CalendarId, 
                         DayOffWorker.Date AS DateNotAvailable
FROM            Workers  JOIN 
                         DayOffWorker ON Workers.Id = DayOffWorker.IdWorker 
UNION

SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.CalendarId, 
                         DaysJobs.Date AS DateNotAvailable
FROM            Workers  JOIN 
                         DaysJobsWorkers ON Workers.Id = DaysJobsWorkers.IdWorker INNER JOIN
                         DaysJobs ON DaysJobsWorkers.IdDayJob = DaysJobs.Id");
            #endregion

            #region Available Workers

            migrationBuilder.Sql(@"DROP VIEW [AvailableWorkers]");
            migrationBuilder.Sql(@"CREATE VIEW [AvailableWorkers]
AS
SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.CalendarId, AllDates.date
FROM            Workers CROSS JOIN
                         AllDates
WHERE        (NOT EXISTS
                             (SELECT        Id
                               FROM            WorkersNotAvailable
                               WHERE        (Id = Workers.Id) AND (DateNotAvailable = AllDates.date)))
GO
");

            #endregion

            #region Jobs By Date
            migrationBuilder.Sql("DROP VIEW [JobsByDate]");
            migrationBuilder.Sql(@"CREATE VIEW [JobsByDate]
AS
SELECT        Jobs.Id, Jobs.Name, Jobs.Number, Jobs.CalendarId, Jobs.Notes, DaysJobs.Date
FROM            Jobs INNER JOIN
                         DaysJobs ON Jobs.Id = DaysJobs.IdJob");

            #endregion

            #region Jobs By Date Worker
            migrationBuilder.Sql("DROP VIEW [JobsByDateWorker]");
            migrationBuilder.Sql(@"CREATE VIEW [JobsByDateWorker]
AS
SELECT        Jobs.Id, Jobs.Name, Jobs.Number, Jobs.CalendarId, Jobs.Notes, DaysJobs.Date, DaysJobsWorkers.IdWorker
FROM            Jobs INNER JOIN
                         DaysJobs ON Jobs.Id = DaysJobs.IdJob INNER JOIN
                         DaysJobsWorkers ON DaysJobs.Id = DaysJobsWorkers.IdDayJob");

            #endregion

            #region Tags By Job
            migrationBuilder.Sql("DROP VIEW [TagsByJob]");
            migrationBuilder.Sql(@"CREATE VIEW [TagsByJob]
AS
SELECT        Tags.Id, Tags.Color, Tags.Description, Tags.Icon, Jobs.Id AS IdJob, Jobs.CalendarId, CAST(0 as BIT) as FromJobDay
FROM            Tags INNER JOIN
                         JobTags ON Tags.Id = JobTags.IdTag INNER JOIN
                         Jobs ON JobTags.IdJob = Jobs.Id");

            #endregion

            #region Tags By Job Date
            migrationBuilder.Sql("DROP VIEW [TagsByJobDate]");
            migrationBuilder.Sql(@"CREATE VIEW [TagsByJobDate]
AS
SELECT        Tags.Id, Tags.Color, Tags.Description, Tags.Icon, DaysJobs.Date, Jobs.Id AS IdJob, Jobs.CalendarId, CAST(0 as BIT) as FromJobDay
FROM            Tags INNER JOIN
                         JobTags ON Tags.Id = JobTags.IdTag INNER JOIN
                         DaysJobs ON DaysJobs.IdJob = JobTags.IdJob INNER JOIN
                         Jobs ON JobTags.IdJob = Jobs.Id
UNION 
SELECT        Tags.Id, Tags.Color, Tags.Description, Tags.Icon, DaysJobs.Date, Jobs.Id AS IdJob, Jobs.CalendarId, CAST(1 as BIT) as FromJobDay
FROM            Tags INNER JOIN
                         DaysJobsTags ON Tags.Id = DaysJobsTags.IdTag INNER JOIN
                         DaysJobs ON DaysJobs.Id= DaysJobsTags.IdDayJob INNER JOIN
                         Jobs ON DaysJobs.IdJob = Jobs.Id");

            #endregion

            #region Time Off Workers
            migrationBuilder.Sql("DROP VIEW [TimeOffWorkers]");
            migrationBuilder.Sql(@"CREATE VIEW [TimeOffWorkers]
AS
SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.CalendarId, DayOffWorker.Date
FROM            Workers INNER JOIN
                         DayOffWorker ON Workers.Id = DayOffWorker.IdWorker");

            #endregion

            #region Workers By Job
            migrationBuilder.Sql("DROP VIEW [WorkersByJob]");
            migrationBuilder.Sql(@"CREATE VIEW [WorkersByJob]
AS
SELECT DISTINCT 
                         Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.CalendarId, DaysJobs.IdJob
FROM            Workers INNER JOIN
                         DaysJobsWorkers ON Workers.Id = DaysJobsWorkers.IdWorker INNER JOIN
                         DaysJobs ON DaysJobs.Id = DaysJobsWorkers.IdDayJob");

            #endregion

            #region Workers By Job Date

            migrationBuilder.Sql("DROP VIEW [WorkersByJobDate]");
            migrationBuilder.Sql(@"CREATE VIEW [WorkersByJobDate]
AS
SELECT DISTINCT 
                         Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.CalendarId, DaysJobs.IdJob, 
                         DaysJobs.Date
FROM            Workers INNER JOIN
                         DaysJobsWorkers ON Workers.Id = DaysJobsWorkers.IdWorker INNER JOIN
                         DaysJobs ON DaysJobs.Id = DaysJobsWorkers.IdDayJob");

            #endregion
        }

        private void DropAndCreateViewsRollback(MigrationBuilder migrationBuilder)
        {
            #region WorkersNotAvailable

            migrationBuilder.Sql("DROP VIEW [WorkersNotAvailable]");
            migrationBuilder.Sql(@"CREATE VIEW [WorkersNotAvailable]
AS
SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.OrganizationId, 
                         DayOffWorker.Date AS DateNotAvailable
FROM            Workers  JOIN 
                         DayOffWorker ON Workers.Id = DayOffWorker.IdWorker 
UNION

SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.OrganizationId, 
                         DaysJobs.Date AS DateNotAvailable
FROM            Workers  JOIN 
                         DaysJobsWorkers ON Workers.Id = DaysJobsWorkers.IdWorker INNER JOIN
                         DaysJobs ON DaysJobsWorkers.IdDayJob = DaysJobs.Id");
            #endregion

            #region Available Workers

            migrationBuilder.Sql(@"DROP VIEW [AvailableWorkers]");
            migrationBuilder.Sql(@"CREATE VIEW [AvailableWorkers]
AS
SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.OrganizationId, Calendar.date
FROM            Workers CROSS JOIN
                         Calendar
WHERE        (NOT EXISTS
                             (SELECT        Id
                               FROM            WorkersNotAvailable
                               WHERE        (Id = Workers.Id) AND (DateNotAvailable = Calendar.date)))
GO
");

            #endregion

            #region Jobs By Date
            migrationBuilder.Sql("DROP VIEW [JobsByDate]");
            migrationBuilder.Sql(@"CREATE VIEW [JobsByDate]
AS
SELECT        Jobs.Id, Jobs.Name, Jobs.Number, Jobs.OrganizationId, Jobs.Notes, DaysJobs.Date
FROM            Jobs INNER JOIN
                         DaysJobs ON Jobs.Id = DaysJobs.IdJob");

            #endregion

            #region Jobs By Date Worker
            migrationBuilder.Sql("DROP VIEW [JobsByDateWorker]");
            migrationBuilder.Sql(@"CREATE VIEW [JobsByDateWorker]
AS
SELECT        Jobs.Id, Jobs.Name, Jobs.Number, Jobs.OrganizationId, Jobs.Notes, DaysJobs.Date, DaysJobsWorkers.IdWorker
FROM            Jobs INNER JOIN
                         DaysJobs ON Jobs.Id = DaysJobs.IdJob INNER JOIN
                         DaysJobsWorkers ON DaysJobs.Id = DaysJobsWorkers.IdDayJob");

            #endregion

            #region Tags By Job
            migrationBuilder.Sql("DROP VIEW [TagsByJob]");
            migrationBuilder.Sql(@"CREATE VIEW [TagsByJob]
AS
SELECT        Tags.Id, Tags.Color, Tags.Description, Tags.Icon, Jobs.Id AS IdJob, Jobs.OrganizationId, CAST(0 as BIT) as FromJobDay
FROM            Tags INNER JOIN
                         JobTags ON Tags.Id = JobTags.IdTag INNER JOIN
                         Jobs ON JobTags.IdJob = Jobs.Id");

            #endregion

            #region Tags By Job Date
            migrationBuilder.Sql("DROP VIEW [TagsByJobDate]");
            migrationBuilder.Sql(@"CREATE VIEW [TagsByJobDate]
AS
SELECT        Tags.Id, Tags.Color, Tags.Description, Tags.Icon, DaysJobs.Date, Jobs.Id AS IdJob, Jobs.OrganizationId, CAST(0 as BIT) as FromJobDay
FROM            Tags INNER JOIN
                         JobTags ON Tags.Id = JobTags.IdTag INNER JOIN
                         DaysJobs ON DaysJobs.IdJob = JobTags.IdJob INNER JOIN
                         Jobs ON JobTags.IdJob = Jobs.Id
UNION 
SELECT        Tags.Id, Tags.Color, Tags.Description, Tags.Icon, DaysJobs.Date, Jobs.Id AS IdJob, Jobs.OrganizationId, CAST(1 as BIT) as FromJobDay
FROM            Tags INNER JOIN
                         DaysJobsTags ON Tags.Id = DaysJobsTags.IdTag INNER JOIN
                         DaysJobs ON DaysJobs.Id= DaysJobsTags.IdDayJob INNER JOIN
                         Jobs ON DaysJobs.IdJob = Jobs.Id");

            #endregion

            #region Time Off Workers
            migrationBuilder.Sql("DROP VIEW [TimeOffWorkers]");
            migrationBuilder.Sql(@"CREATE VIEW [TimeOffWorkers]
AS
SELECT        Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.OrganizationId, DayOffWorker.Date
FROM            Workers INNER JOIN
                         DayOffWorker ON Workers.Id = DayOffWorker.IdWorker");

            #endregion

            #region Workers By Job
            migrationBuilder.Sql("DROP VIEW [WorkersByJob]");
            migrationBuilder.Sql(@"CREATE VIEW [WorkersByJob]
AS
SELECT DISTINCT 
                         Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.OrganizationId, DaysJobs.IdJob
FROM            Workers INNER JOIN
                         DaysJobsWorkers ON Workers.Id = DaysJobsWorkers.IdWorker INNER JOIN
                         DaysJobs ON DaysJobs.Id = DaysJobsWorkers.IdDayJob");

            #endregion

            #region Workers By Job Date

            migrationBuilder.Sql("DROP VIEW [WorkersByJobDate]");
            migrationBuilder.Sql(@"CREATE VIEW [WorkersByJobDate]
AS
SELECT DISTINCT 
                         Workers.Id, Workers.Email, Workers.FirstName, Workers.LastName, Workers.Phone, Workers.OrganizationId, DaysJobs.IdJob, 
                         DaysJobs.Date
FROM            Workers INNER JOIN
                         DaysJobsWorkers ON Workers.Id = DaysJobsWorkers.IdWorker INNER JOIN
                         DaysJobs ON DaysJobs.Id = DaysJobsWorkers.IdDayJob");

            #endregion
        }
    }
}
