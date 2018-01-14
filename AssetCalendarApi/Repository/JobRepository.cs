using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Repository
{
    public class JobRepository
    {
        private AssetCalendarDbContext _dbContext;

        public JobRepository(AssetCalendarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Job> GetAllJobs()
        {
            return _dbContext.Jobs;
        }

        public Job GetJob(Guid id)
        {
            return _dbContext.Jobs.FirstOrDefault(j => j.Id == id);
        }

        public IQueryable<Job> GetJobsForDay(DateTime date)
        {
            return
                _dbContext.Jobs
                    .Join(_dbContext.DaysJobs,
                        d => d.Id,
                        j => j.IdJob,
                        (job, dayJob) => new { dayJob.Date, job })
                    .Where(m => m.Date == date)
                    .Select(m => m.job);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForMonth(DateTime month)
        {
            return
                _dbContext.Jobs
                        .Join(_dbContext.DaysJobs,
                            d => d.Id,
                            j => j.IdJob,
                            (job, dayJob) => new { dayJob.Date, job })
                        .Where(m => m.Date.Month == month.Month)
                        .GroupBy(m => m.Date)
                        .ToDictionary(group => group.Key, group => group.Select(g => g.job));
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForWeek(DateTime week)
        {
            return
                _dbContext.Jobs
                    .Join(_dbContext.DaysJobs,
                        d => d.Id,
                        j => j.IdJob,
                        (job, dayJob) => new { dayJob.Date, job })
                    .Where(m => m.Date >= week.StartOfWeek() && m.Date <= week.EndOfWeek())
                    .GroupBy(m => m.Date)
                    .ToDictionary(group => group.Key, group => group.Select(g => g.job));
        }

        public Job AddJob(AddJobModel model)
        {
            Job job = new Job()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Number = model.Number,
                Type = model.Type
            };

            _dbContext.Jobs.Add(job);

            for (DateTime date = model.StartDate.Date; date <= (model.EndDate.HasValue ? model.EndDate.Value.Date : model.StartDate.Date); date = date.AddDays(1))
            {
                var dj = new DayJob()
                {
                    Id = Guid.NewGuid(),
                    IdJob = job.Id,
                    Date = date
                };

                _dbContext.DaysJobs.Add(dj);

                foreach( var id in model.WorkerIds)
                {
                    var djw = new DayJobWorker()
                    {
                        Id = Guid.NewGuid(),
                        IdDayJob = dj.Id,
                        IdWorker = new Guid(id)
                    };

                    _dbContext.DaysJobsWorkers.Add(djw);
                }
            }

            _dbContext.SaveChanges();

            return job;
        }

        public void AddJobToDay(Guid idJob, DateTime date)
        {
            var dayJob = _dbContext.DaysJobs.FirstOrDefault(dj => dj.IdJob == idJob && dj.Date == date);
            if (dayJob != null)
                return;

            _dbContext.DaysJobs.Add(new DayJob()
            {
                IdJob = idJob,
                Date = date
            });

            _dbContext.SaveChanges();
        }

        public void AddWorkerToJob(Guid idJob, Guid idWorker, DateTime date)
        {
            AddWorkerToJobs(idJob, idWorker, date);
        }

        public void AddWorkerToJob(Guid idJob, Guid idWorker)
        {
            AddWorkerToJobs(idJob, idWorker, null );
        }

        private void AddWorkerToJobs( Guid idJob, Guid idWorker, DateTime? date )
        {
            var jobDays = _dbContext.DaysJobs.Where(d => d.IdJob == idJob);
            if (date.HasValue)
                jobDays = jobDays.Where(d => d.Date == date.Value);

            var workerDays = _dbContext.DaysJobsWorkers.Where(w => w.IdWorker == idWorker);

            var notWorking = jobDays.Where(j => !workerDays.Any(w => w.IdDayJob == j.Id));

            foreach( var jobDay in notWorking)
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

        public void DeleteJob( string idJob)
        {
            var job = GetJob(new Guid(idJob));

            _dbContext.Jobs.Remove(job);

            _dbContext.SaveChanges();
        }
    }
}
