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

        public IEnumerable<Worker> GetAvailableWorkersForDay(Guid organizationId, DateTime startDate )
        {
            var avail = GetAvailableWorkersForDates(organizationId, startDate, null );

            return (avail.ContainsKey(startDate.Date) ? avail[startDate.Date] : Enumerable.Empty<Worker>());
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

            var availView =_dbContext.AvailableWorkers
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Value.Date && a.OrganizationId == organizationId)
                .GroupBy(a => a.Date.Date)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(avail => AutoMapper.Mapper.Map<Worker>(avail)));

            return availView;
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

            return _dbContext.TimeOffWorkers.Where( t => t.OrganizationId == organizationId && t.Date >= startDate.Date && t.Date <= endDate.Value.Date )
                .GroupBy(t => t.Date.Date)
                .ToDictionary(group => group.Key, group => group.Select( t => AutoMapper.Mapper.Map<Worker>(t) ));
        }

        public Dictionary<Guid, IEnumerable<Worker>> GetWorkersByJob( DateTime? date, Guid organizationId)
        {
           
            if (date.HasValue)
            {
                return _dbContext.WorkersByJobDate
                    .Where(w => w.OrganizationId == organizationId && w.Date.Date == date.Value.Date)
                    .GroupBy(w => w.IdJob)
                    .ToDictionary(group => group.Key, group => group.Select(w => AutoMapper.Mapper.Map<Worker>(w)));
            }

            return _dbContext.WorkersByJobDate
                    .Where(w => w.OrganizationId == organizationId )
                    .GroupBy(w => w.IdJob)
                    .ToDictionary(group => group.Key, group => group.Select(w => AutoMapper.Mapper.Map<Worker>(w)));
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
