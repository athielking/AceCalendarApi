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
        #region Data Members

        private readonly AssetCalendarDbContext _dbContext;

        private readonly JobRepository _jobRepository;

        private readonly TagRepository _tagRepository;

        #endregion

        #region Constructor

        public WorkerRepository
        (
            AssetCalendarDbContext dbContext,
            JobRepository jobRepository,
            TagRepository tagRepository
        )
        {
            _dbContext = dbContext;
            _jobRepository = jobRepository;
            _tagRepository = tagRepository;
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
            return GetWorkersByOrganization(organizationId)
                .Include(w => w.WorkerTags)
                .ThenInclude(t => t.Tag);
        }

        public Worker GetWorker(Guid id, Guid organizationId)
        {
            return GetWorkersByOrganization(organizationId)
                .Include(w => w.WorkerTags)
                .ThenInclude(t => t.Tag)
                .AsExpandable()
                .FirstOrDefault(w => w.Id == id);
        }

        public IEnumerable<DayOffWorker> GetTimeOffForMonth(Guid workerId, DateTime date, Guid organizationId)
        {
            return _dbContext.DayOffWorkers
                .Include(dayOffWorker => dayOffWorker.Worker)
                .Where
                (
                    dayOffWorker =>
                        dayOffWorker.IdWorker == workerId &&
                        dayOffWorker.Date.Month == date.Month &&
                        dayOffWorker.Date.Year == date.Year &&
                        dayOffWorker.Worker.OrganizationId == organizationId
                );
        }

        public DayOffWorker GetTimeOffForDay(Guid workerId, DateTime date, Guid organizationId)
        {
            return _dbContext.DayOffWorkers
                .Include(dayOffWorker => dayOffWorker.Worker)
                .SingleOrDefault
                (
                    dayOffWorker =>
                        dayOffWorker.IdWorker == workerId &&
                        dayOffWorker.Date.Day == date.Day &&
                        dayOffWorker.Date.Month == date.Month &&
                        dayOffWorker.Date.Year == date.Year &&
                        dayOffWorker.Worker.OrganizationId == organizationId
                );
        }

        public IEnumerable<DayJobWorker> GetJobsForMonth(Guid id, DateTime date, Guid organizationId)
        {
            return _dbContext.DaysJobsWorkers
                .Include(dayJobWorker => dayJobWorker.Worker)
                .Include(dayJobWorker => dayJobWorker.DayJob)
                .ThenInclude(daysJob => daysJob.Job)
                .Where
                (
                    dayJobWorker =>
                        dayJobWorker.Worker.Id == id &&
                        dayJobWorker.DayJob.Date.Month == date.Month &&
                        dayJobWorker.DayJob.Date.Year == date.Year &&
                        dayJobWorker.Worker.OrganizationId == organizationId
                );
        }

        public Worker GetWorkerWithJobs(Guid id, Guid organizationId)
        {
            return GetWorkersByOrganization(organizationId)
                .Include(worker => worker.DayJobWorkers)
                .ThenInclude(djw => djw.DayJob)
                .AsExpandable()
                .FirstOrDefault(w => w.Id == id);
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForDay(Guid organizationId, DateTime startDate)
        {
            return GetAvailableWorkersForDates(organizationId, startDate, null);
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForMonth(Guid organizationId, DateTime dateInMonth)
        {
            //Need to always fill out the entire week of the start and end of the month
            var startOfMonth = new DateTime(dateInMonth.Year, dateInMonth.Month, 1).StartOfWeek();
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).EndOfWeek();

            return GetAvailableWorkersForDates(organizationId, startOfMonth, endOfMonth);
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForWeek(Guid organizationId, DateTime dateInWeek)
        {
            return GetAvailableWorkersForDates(organizationId, dateInWeek.StartOfWeek(), dateInWeek.EndOfWeek());
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForDates(Guid organizationId, DateTime startDate, DateTime? endDate)
        {
            endDate = endDate ?? startDate;

            var avail = _dbContext.AvailableWorkers
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Value.Date && a.OrganizationId == organizationId);

            var dictionary = new Dictionary<DateTime, IEnumerable<Worker>>();
            var keys = avail.Select(w => w.Date).Distinct();

            foreach (var k in keys)
            {
                dictionary.Add(k, avail.Where(a => a.Date == k).Select(a => AutoMapper.Mapper.Map<Worker>(a)));
            }

            return dictionary;
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetOffWorkersForMonth(Guid organizationId, DateTime dateInMonth)
        {
            //Need to always fill out the entire week of the start and end of the month
            var startOfMonth = new DateTime(dateInMonth.Year, dateInMonth.Month, 1).StartOfWeek();
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).EndOfWeek();

            return GetOffWorkersForDates(organizationId, startOfMonth, endOfMonth);
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetOffWorkersForWeek(Guid organizationId, DateTime dateInWeek)
        {
            return GetOffWorkersForDates(organizationId, dateInWeek.StartOfWeek(), dateInWeek.EndOfWeek());
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetOffWorkersForDates(Guid organizationId, DateTime startDate, DateTime? endDate)
        {
            endDate = endDate ?? startDate;

            var off = _dbContext.DayOffWorkers
                .Include( w => w.Worker )
                .Where(t => t.Worker.OrganizationId == organizationId && t.Date >= startDate.Date && t.Date <= endDate.Value.Date);

            var dictionary = new Dictionary<DateTime, IEnumerable<Worker>>();
            var keys = off.Select(w => w.Date).Distinct();

            foreach (var k in keys)
            {
                dictionary.Add(k, off.Where(a => a.Date == k).Select( o => o.Worker ));
            }

            return dictionary;
        }

        public Dictionary<Guid, IEnumerable<Worker>> GetWorkersByJob(DateTime? date, Guid organizationId)
        {
            var workers = _dbContext.WorkersByJobDate.Where(w => w.OrganizationId == organizationId);

            if (date.HasValue)
                workers = workers.Where(w => w.Date.Date == date.Value.Date);

            var dictionary = new Dictionary<Guid, IEnumerable<Worker>>();
            var keys = workers.Select(w => w.IdJob).Distinct();

            foreach (var k in keys)
                dictionary.Add(k, workers.Where(w => w.IdJob == k).Select(w => AutoMapper.Mapper.Map<Worker>(w)));

            return dictionary;
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

            foreach (var tag in worker.Tags)
                _tagRepository.AddTagToWorker(tag.Id, dbWorker.Id);

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

            //Update Tags
            _tagRepository.UpdateTagsForWorker(id, workerViewModel.Tags);

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

        public void DeleteTimeOff(Guid workerId, DateTime date, Guid organizationId)
        {
            var timeOff = GetTimeOffForDay(workerId, date, organizationId);

            if (timeOff == null)
                throw new ApplicationException("Time Off Entry not Found");

            _dbContext.DayOffWorkers.Remove(timeOff);
            _dbContext.SaveChanges();
        }

        public void EditTimeOff(Guid workerId, DateTime monthDate, IEnumerable<DateTime> timeOffDates, Guid organizationId)
        {
            var dayOffWorkers = GetTimeOffForMonth(workerId, monthDate, organizationId);

            //Remove existing time off days not in the new set
            foreach (var dayOffWorker in dayOffWorkers)
            {
                if (!timeOffDates.Any( d => d.Date == dayOffWorker.Date.Date))
                    _dbContext.DayOffWorkers.Remove(dayOffWorker);
            }

            _dbContext.SaveChanges();

            var existingTimeOffDates = dayOffWorkers.Select(dayOffWorker => dayOffWorker.Date.Date);

            //Add new time off days not in the existing set
            foreach (var timeOffDate in timeOffDates)
            {
                if (existingTimeOffDates.Contains(timeOffDate.Date))
                    continue;

                _jobRepository.MoveWorkerToOff(workerId, timeOffDate.Date, organizationId);
            }
        }

        #endregion
    }
}
