using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using System.Linq.Expressions;

namespace AssetCalendarApi.Repository
{
    public class WorkerRepository
    {
        #region Private Methods

        private readonly AssetCalendarDbContext _dbContext;

        #endregion

        #region Constructor

        public WorkerRepository
        (
            AssetCalendarDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }

        #endregion

        #region Private Methods

        private IQueryable<Worker> GetWorkersByOrganization(Guid organizationId)
        {
            return _dbContext.Workers
                .Where(worker => worker.OrganizationId == organizationId);
        }

        #endregion

        #region Public Methods

        public IQueryable<Worker> GetAllWorkers(Guid organizationId)
        {
            return GetWorkersByOrganization(organizationId);
        }

        public Worker GetWorker(Guid id, Guid organizationId)
        {
            return GetWorkersByOrganization(organizationId)
                .Include( w => w.DayOffWorkers )
                .Include( w => w.DayJobWorkers )
                .ThenInclude( djw => djw.DayJob )
                .ThenInclude( dj => dj.Job )
                .AsExpandable()
                .FirstOrDefault(w => w.Id == id);
        }

        public Worker GetWorkerWithJobs(Guid id, Guid organizationId)
        {
            return GetWorkersByOrganization(organizationId)
                .Include(worker => worker.DayJobWorkers)
                .ThenInclude(djw => djw.DayJob)
                .AsExpandable()
                .FirstOrDefault(w => w.Id == id);
        }

        public IQueryable<Worker> GetAvailableWorkers(Guid organizationId, DateTime startDate, DateTime? endDate = null)
        {
            endDate = endDate ?? startDate;

            return GetWorkersByOrganization(organizationId)
               .AsExpandable()
               .Except
               (
                    _dbContext.DaysJobs
                        .Where(dj => dj.Date.Date >= startDate.Date && dj.Date.Date <= endDate.Value.Date)
                        .Join
                        (
                            _dbContext.DaysJobsWorkers,
                            dj => dj.Id,
                            djw => djw.IdDayJob,
                            (dayJob, dayJobWorker) => dayJobWorker.Worker
                        )
                );
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForMonth(Guid organizationId, DateTime dateInMonth)
        {
            var startOfMonth = new DateTime(dateInMonth.Year, dateInMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return GetAvailableWorkersForDates(organizationId, startOfMonth, endOfMonth);
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForWeek(Guid organizationId, DateTime dateInWeek)
        {
            return GetAvailableWorkersForDates(organizationId, dateInWeek.StartOfWeek(), dateInWeek.EndOfWeek());
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForDates(Guid organizationId, DateTime startDate, DateTime? endDate)
        {
            endDate = endDate ?? startDate;

            var allWorkers = GetWorkersByOrganization(organizationId);
            var allDates = startDate.GetDatesTo(endDate);

            //Get the cartesian product for all dates + all workers
            var available =
                from worker in allWorkers
                from date in allDates
                select new { date, worker };

            //Get a dictionary of who is working on what day
            var working = _dbContext.DaysJobs.Where(dj => dj.Date >= startDate && dj.Date <= allDates.Last())
                    .Join(_dbContext.DaysJobsWorkers,
                        dj => dj.Id,
                        djw => djw.IdDayJob,
                        (dayJob, dayJobWorker) => new { dayJob.Date, dayJobWorker.IdWorker })
                    .GroupBy(m => m.Date)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(m => m.IdWorker));

            //Get a dictionary of who is off for what day
            var off = _dbContext.DayOffWorkers.Where(dow => dow.Date >= startDate && dow.Date <= allDates.Last())
                .GroupBy(m => m.Date.Date)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(m => m.IdWorker));

            //Get a dictionary of dates, and who is not already working
            var availableWorkers = available
                .GroupBy(m => m.date)
                .ToDictionary(
                    group => group.Key,
                    group => group.Where(
                        m => (!working.ContainsKey(group.Key) || (working.ContainsKey(group.Key) && !working[group.Key].Contains(m.worker.Id))) &&
                            (!off.ContainsKey(group.Key) || (off.ContainsKey(group.Key) && !off[group.Key].Contains(m.worker.Id))))
                .Select(m => m.worker));

            return availableWorkers;
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetOffWorkersForMonth(Guid organizationId, DateTime dateInMonth)
        {
            var startOfMonth = new DateTime(dateInMonth.Year, dateInMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return GetOffWorkersForDates(organizationId, startOfMonth, endOfMonth);
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetOffWorkersForWeek(Guid organizationId, DateTime dateInWeek)
        {
            return GetOffWorkersForDates(organizationId, dateInWeek.StartOfWeek(), dateInWeek.EndOfWeek());
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetOffWorkersForDates(Guid organizationId, DateTime startDate, DateTime? endDate)
        {
            endDate = endDate ?? startDate;

            return _dbContext.DayOffWorkers
                .Include(dow => dow.Worker)
                .Where(dow => dow.Date.Date >= startDate.Date && dow.Date.Date <= endDate.Value.Date)
                .GroupBy(dow => dow.Date.Date)
                .ToDictionary(group => group.Key, group => group.Select(dow => dow.Worker));
        }

        public IQueryable<Worker> GetWorkersForJob(Guid idJob, DateTime? date, Guid organizationId)
        {
            //Make One Repository
            //var job = _jobRepository.GetJob(idJob, organizationId);

            //if (job == null)
            //    throw new ApplicationException( "Unable to Locate Job" );

            //Make this a repository method
            var dayJobs = _dbContext.DaysJobs.Where(dj => dj.IdJob == idJob);

            if (date.HasValue)
                dayJobs = dayJobs.Where(dj => dj.Date.Date == date.Value.Date);

            return dayJobs.SelectMany(dj => dj.DayJobWorkers.Select(djw => djw.Worker)).Distinct();
        }

        public Worker AddWorker(WorkerViewModel worker, Guid organizationId)
        {
            var dbWorker = new Worker()
            {
                Id = Guid.NewGuid(),
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Email = worker.Email,
                Phone = worker.Phone,
                OrganizationId = organizationId
            };

            _dbContext.Workers.Add(dbWorker);
            _dbContext.SaveChanges();

            return dbWorker;
        }

        public void EditWorker(Guid id, WorkerViewModel workerViewModel, Guid organizationId)
        {
            var worker = GetWorkersByOrganization(organizationId).FirstOrDefault(w => w.Id == id);

            if (worker == null)
                throw new ApplicationException("Worker not Found");

            worker.FirstName = workerViewModel.FirstName;
            worker.LastName = workerViewModel.LastName;
            worker.Email = workerViewModel.Email;
            worker.Phone = workerViewModel.Phone;

            _dbContext.Workers.Update(worker);
            _dbContext.SaveChanges();
        }

        public void DeleteWorker(Guid id, Guid organizationId)
        {
            var worker = GetWorkersByOrganization(organizationId).FirstOrDefault(w => w.Id == id);

            if (worker == null)
                throw new ApplicationException("Worker not Found");

            _dbContext.Workers.Remove(worker);
            _dbContext.SaveChanges();
        }

        public Worker AddTimeOff(DateRangeViewModel model, Guid organizationId)
        {
            var worker = GetWorker(model.Id, organizationId);

            _dbContext.Attach(worker);

            var end = model.End ?? model.Date;

            foreach(var d in model.Date.GetDatesTo(end) )
            {
                var dayOff = new DayOffWorker()
                {
                    Id = Guid.NewGuid(),
                    Date = d,
                    IdWorker = model.Id
                };

                if( !worker.DayOffWorkers.Any( off => off.Date.Date == d.Date ))
                    worker.DayOffWorkers.Add( dayOff );
            }

            _dbContext.SaveChanges();

            return worker;
        }

        public Worker DeleteTimeOff(DateRangeViewModel model, Guid organizationId)
        {
            var worker = GetWorker(model.Id, organizationId);
            _dbContext.Attach(worker);

            var end = model.End ?? model.Date;

            foreach (var d in model.Date.GetDatesTo(end))
            {
                var off = worker.DayOffWorkers.FirstOrDefault(dow => dow.Date.Date == d.Date);
                if (off != null)
                    worker.DayOffWorkers.Remove(off);
            }

            _dbContext.SaveChanges();

            return worker;
        }
        #endregion
    }
}
