using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;

namespace AssetCalendarApi.Repository
{
    public class JobRepository
    {
        #region Data Members

        private readonly AssetCalendarDbContext _dbContext;

        #endregion

        #region Constructor

        public JobRepository
        (
            AssetCalendarDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }

        #endregion

        #region Private Methods
      
        private IQueryable<Job> GetJobsByOrganization(Guid organizationId)
        {
            return _dbContext.Jobs
                .Where(job => job.OrganizationId == organizationId);
        }

        #endregion

        #region Public Methods

        public IQueryable<Job> GetAllJobs(Guid organizationId)
        {
            return GetJobsByOrganization(organizationId);
        }

        public Job GetJob(Guid id, Guid organizationId)
        {
            return GetJobsByOrganization(organizationId)
                .AsExpandable()
                .FirstOrDefault(job => job.Id == id);
        }

        public IQueryable<Job> GetJobsForDay(DateTime date, Guid organizationId)
        {
            return GetJobsByOrganization(organizationId)
                .AsExpandable()
                .Join(_dbContext.DaysJobs,
                    d => d.Id,
                    j => j.IdJob,
                    (job, dayJob) => new { dayJob.Date, job })
                .Where(m => m.Date == date)
                .Select(m => m.job);
        }

        public IQueryable<DayJob> GetDayJobsForDay(DateTime date, Guid organizationId)
        {
            return GetJobsByOrganization(organizationId)
                .Include(j => j.DaysJobs)
                .SelectMany(j => j.DaysJobs)
                .Where(dj => dj.Date.Date == date.Date);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForMonth(DateTime month, Guid organizationId)
        {
            return GetJobsByOrganization(organizationId)
                .AsExpandable()
                .Join(_dbContext.DaysJobs,
                    d => d.Id,
                    j => j.IdJob,
                    (job, dayJob) => new { dayJob.Date, job })
                .Where(m => m.Date.Month == month.Month)
                .GroupBy(m => m.Date)
                .ToDictionary(group => group.Key, group => group.Select(g => g.job));
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForWeek(DateTime week, Guid organizationId)
        {
            return GetJobsByOrganization(organizationId)
                .AsExpandable()
                .Join(_dbContext.DaysJobs,
                    d => d.Id,
                    j => j.IdJob,
                    (job, dayJob) => new { dayJob.Date, job })
                .Where(m => m.Date >= week.StartOfWeek() && m.Date <= week.EndOfWeek())
                .GroupBy(m => m.Date)
                .ToDictionary(group => group.Key, group => group.Select(g => g.job));
        }

        public Job AddJob(AddJobModel addJobModel, Guid organizationId)
        {
            var job = new Job()
            {
                Id = Guid.NewGuid(),
                Name = addJobModel.Name,
                Number = addJobModel.Number,
                Notes = addJobModel.Notes,
                OrganizationId = organizationId
            };

            _dbContext.Jobs.Add(job);

            for (var date = addJobModel.StartDate.Date; date <= addJobModel.EndDate; date = date.AddDays(1))
            {
                var dayJob = new DayJob()
                {
                    Id = Guid.NewGuid(),
                    IdJob = job.Id,
                    Date = date
                };

                _dbContext.DaysJobs.Add(dayJob);
            }

            _dbContext.SaveChanges();

            return job;
        }

        //public void AddJobToDay(Guid idJob, DateTime date, Guid organizationId)
        //{
        //    var dayJob = _dbContext.DaysJobs.FirstOrDefault(dj => dj.IdJob == idJob && dj.Date == date);
        //    if (dayJob != null)
        //        return;

        //    _dbContext.DaysJobs.Add(new DayJob()
        //    {
        //        IdJob = idJob,
        //        Date = date
        //    });

        //    _dbContext.SaveChanges();
        //}

        public void AddWorkerToJob(Guid idJob, Guid idWorker, Guid organizationId, DateTime? date = null)
        {
            var job = GetJob(idJob, organizationId);

            if (job == null)
                throw new ApplicationException("Unable to Locate Job");

            //Make One Repository
            //var worker = _workerRepository.GetWorker(idWorker, organizationId);

            //if (worker == null)
            //    throw new ApplicationException("Unable to Locate Worker");

            //Make this a repository method
            var jobDays = _dbContext.DaysJobs.Where(d => d.IdJob == job.Id);

            if (date.HasValue)
                jobDays = jobDays.Where(d => d.Date.Date == date.Value.Date);

            var workerDays = _dbContext.DaysJobsWorkers.Where(w => w.IdWorker == idWorker);

            var notWorking = jobDays.Where(j => !workerDays.Any(w => w.IdDayJob == j.Id));

            foreach (var jobDay in notWorking)
            {
                var jobDayWorker = new DayJobWorker()
                {
                    IdDayJob = jobDay.Id,
                    IdWorker = idWorker
                };

                _dbContext.DaysJobsWorkers.Add(jobDayWorker);
            }

            _dbContext.SaveChanges();
        }

        public void DeleteJob(Guid idJob, Guid organizationId)
        {
            var job = GetJob(idJob, organizationId);

            _dbContext.Jobs.Remove(job);

            _dbContext.SaveChanges();
        }

        public void MoveWorkerToJob(Guid idJob, Guid idWorker, DateTime date, Guid organizationId)
        {
            //Make into repository method that only includes jobs for the given organization           
            var jobsOnDay = _dbContext.DaysJobs.Include(dj => dj.DayJobWorkers).Where(dj => dj.Date.Date == date.Date);

            var fromJob = jobsOnDay.FirstOrDefault(dj => dj.DayJobWorkers.Any(djw => djw.IdWorker == idWorker));
            var toJob = jobsOnDay.FirstOrDefault(j => j.IdJob == idJob);

            var dayOff = _dbContext.DayOffWorkers.FirstOrDefault(dow => dow.IdWorker == idWorker && dow.Date.Date == date.Date);
            if(dayOff != null)
                _dbContext.DayOffWorkers.Remove(dayOff);

            if (fromJob != null)
            {
                var existingWorkerDay = fromJob.DayJobWorkers.FirstOrDefault(djw => djw.IdWorker == idWorker);

                existingWorkerDay.IdDayJob = toJob.Id;
                _dbContext.SaveChanges();
                return;
            }

            AddWorkerToJob(idJob, idWorker, organizationId, date);
        }

        public void MoveWorkerToOff(Guid idWorker, DateTime date, Guid organizationId)
        {
            //Make into repository method that only includes jobs for the given organization           
            var jobsOnDay = _dbContext.DaysJobs.Include(dj => dj.DayJobWorkers).Where(dj => dj.Date.Date == date.Date);

            var fromJob = jobsOnDay.FirstOrDefault(dj => dj.DayJobWorkers.Any(djw => djw.IdWorker == idWorker));
          
            if (fromJob != null)
            {
                var existingWorkerDay = fromJob.DayJobWorkers.FirstOrDefault(djw => djw.IdWorker == idWorker);
                _dbContext.DaysJobsWorkers.Remove(existingWorkerDay);
            }

            var dayOff = new DayOffWorker()
            {
                Id = Guid.NewGuid(),
                IdWorker = idWorker,
                Date = date
            };

            _dbContext.DayOffWorkers.Add(dayOff);
            _dbContext.SaveChanges();
        }

        public void MakeWorkerAvailable(Guid idWorker, DateTime date, Guid organizationId)
        {
            var jobWorker = GetDayJobsForDay(date, organizationId)
                .Include(dj => dj.DayJobWorkers)
                .SelectMany(dj => dj.DayJobWorkers)
                .FirstOrDefault(w => w.IdWorker == idWorker);

            if (jobWorker != null)
                _dbContext.Remove(jobWorker);

            var dayOffWorker = _dbContext.DayOffWorkers.FirstOrDefault(dow => dow.IdWorker == idWorker && dow.Date.Date == date.Date);

            if (dayOffWorker != null)
                _dbContext.DayOffWorkers.Remove(dayOffWorker);

            _dbContext.SaveChanges();
        }

        public void SaveNotes(Guid idJob, Guid organizationId, string notes )
        {
            var job = GetJob(idJob, organizationId);

            _dbContext.Attach(job);

            job.Notes = notes;

            _dbContext.SaveChanges();
        }
        #endregion        
    }
}
