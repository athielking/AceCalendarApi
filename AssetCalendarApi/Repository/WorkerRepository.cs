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

        #region Public Methods

        public IQueryable<Worker> GetAllWorkers(Guid organizationId)
        {
            return GetWorkersByOrganization(organizationId)
                .Include(w => w.WorkerTags)
                .ThenInclude(t => t.Tag);
        }

        public IQueryable<Worker> GetWorkersByOrganization(Guid organizationId)
        {
            return _dbContext.Workers
                .Where(worker => worker.OrganizationId == organizationId);
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

        public IEnumerable<Worker> GetAvailableWorkersForDay(Guid organizationId, DateTime startDate)
        {
            var avail = GetAvailableWorkersForDates(organizationId, startDate, null);

            return (avail.ContainsKey(startDate.Date) ? avail[startDate.Date] : Enumerable.Empty<Worker>());
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

            var availView = _dbContext.AvailableWorkers
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Value.Date && a.OrganizationId == organizationId)
                .GroupBy(a => a.Date.Date)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(avail => AutoMapper.Mapper.Map<Worker>(avail)));

            return availView;
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

            return _dbContext.TimeOffWorkers.Where(t => t.OrganizationId == organizationId && t.Date >= startDate.Date && t.Date <= endDate.Value.Date)
                .GroupBy(t => t.Date.Date)
                .ToDictionary(group => group.Key, group => group.Select(t => AutoMapper.Mapper.Map<Worker>(t)));
        }

        public Dictionary<Guid, IEnumerable<Worker>> GetWorkersByJob(DateTime? date, Guid organizationId)
        {

            if (date.HasValue)
            {
                return _dbContext.WorkersByJobDate
                    .Where(w => w.OrganizationId == organizationId && w.Date.Date == date.Value.Date)
                    .GroupBy(w => w.IdJob)
                    .ToDictionary(group => group.Key, group => group.Select(w => AutoMapper.Mapper.Map<Worker>(w)));
            }

            return _dbContext.WorkersByJobDate
                    .Where(w => w.OrganizationId == organizationId)
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
