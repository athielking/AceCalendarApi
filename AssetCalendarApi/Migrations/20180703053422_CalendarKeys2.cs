using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AssetCalendarApi.Migrations
{
    public partial class CalendarKeys2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarUsers");

            migrationBuilder.CreateTable(
               name: "CalendarUsers",
               columns: table => new
               {
                   Id = table.Column<Guid>(nullable: false),
                   CalendarId = table.Column<Guid>(nullable: false),
                   UserId = table.Column<string>(nullable: false, unicode: true, maxLength:450),
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

            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_Organization",
                table: "Calendars");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers");

            //Create our Default Calendars, Since we are Renaming the OrganizationId Columns to CalendarId
            //Use the organization Id to seed the calendar ids
            migrationBuilder.Sql($"INSERT INTO Calendars ( Id, CalendarName, OrganizationId, Inactive ) " +
                "SELECT Id, 'Default Calendar', NEWID(), 0 FROM Organization");

            // Add the users to the default calendar
            migrationBuilder.Sql("INSERT INTO CalendarUsers (Id, UserId, CalendarId ) SELECT NEWID(), Id, OrganizationId FROM AspNetUsers;");

            //Next we update the Organizations with the Id's we generated when we created the calendars
            migrationBuilder.Sql("UPDATE Organization SET Id = OrganizationId FROM Organization INNER JOIN Calendars ON Organization.Id = Calendars.Id");

            //Update the users to have the correct org id
            migrationBuilder.Sql("Update AspNetUsers Set OrganizationId = Calendars.OrganizationId FROM AspNetUsers JOIN Calendars ON AspNetUsers.OrganizationId = Calendars.Id");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarUsers_CalendarId",
                table: "CalendarUsers",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_OrganizationId",
                table: "Calendars",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Calendar",
                table: "Jobs",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Calendar",
                table: "Tags",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Calendar",
                table: "Workers",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_Organization",
                table: "Calendars",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            //Rename the table full of dates so its not confusing
            migrationBuilder.Sql(@"exec sp_rename 'Calendar', 'AllDates'");

            DropAndCreateViewsUpgrade(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
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
    }
}
