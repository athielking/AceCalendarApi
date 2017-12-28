using AssetCalendarApi.Models;
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

        public void AddJob(AddJobModel model)
        {
            Job job = new Job()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Number = model.Number,
                Type = model.Type
            };

            _dbContext.Jobs.Add(job);

            for (DateTime date = model.StartDate; date <= (model.EndDate.HasValue ? model.EndDate : model.StartDate); date = date.AddDays(1) )
            {
                _dbContext.DaysJobs.Add(new DayJob() { IdJob = job.Id, Date = date });
            }
            _dbContext.SaveChanges();
        }

        public void AddJobToDay(Guid idJob, DateTime date )
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

    }
}
