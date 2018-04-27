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
        private readonly TagRepository _tagRepository;
        #endregion

        #region Constructor

        public JobRepository
        (
            AssetCalendarDbContext dbContext,
            TagRepository tagRepository
        )
        {
            _dbContext = dbContext;
            _tagRepository = tagRepository;
        }

        #endregion

        #region Private Methods

        private IQueryable<Job> GetJobsByOrganization(Guid organizationId)
        {
            return _dbContext.Jobs
                .Where(job => job.OrganizationId == organizationId);
        }

        private IQueryable<DayJob> GetDayJobsForJob(Guid jobId, Guid organizationId)
        {
            return GetJobsByOrganization(organizationId)
                .AsExpandable()
                .Where(job => job.Id == jobId)
                .Include(job => job.DaysJobs)
                .SelectMany(job => job.DaysJobs);
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

        public IEnumerable<DayJob> GetJobDaysForJob(Guid jobId, Guid organizationId)
        {
            return _dbContext.DaysJobs.Where(dj => dj.IdJob == jobId);
        }

        public IQueryable<DayJob> GetDayJobsForDay(DateTime date, Guid organizationId)
        {
            return GetJobsByOrganization(organizationId)
                .Include(j => j.DaysJobs)
                .SelectMany(j => j.DaysJobs)
                .Where(dj => dj.Date.Date == date.Date);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForMonth(DateTime month, Guid organizationId, Guid? idWorker = null)
        {
            //Need the calendar to show from sunday to saturday regardless of month
            DateTime monthStart = new DateTime(month.Year, month.Month, 1).StartOfWeek();
            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1).EndOfWeek();

            return GetJobsForRange(organizationId, monthStart, monthEnd, idWorker);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForWeek(DateTime week, Guid organizationId, Guid? idWorker = null)
        {
            return GetJobsForRange(organizationId, week.StartOfWeek(), week.EndOfWeek(), idWorker);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForDay(DateTime date, Guid organizationId, Guid? idWorker = null)
        {
            return GetJobsForRange(organizationId, date, null, idWorker);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForRange( Guid organizationId, DateTime start, DateTime? end, Guid? idWorker = null)
        {
            var startDate = start.Date;
            var endDate = end.HasValue ? end.Value.Date : startDate;

            if (idWorker.HasValue)
            {
                return _dbContext.JobsByDateWorker.Where(
                    j => j.OrganizationId == organizationId &&
                        j.Date >= startDate && j.Date <= endDate &&
                        j.IdWorker == idWorker.Value
                    )
                    .GroupBy(j => j.Date)
                    .ToDictionary(group => group.Key, group => group.Select(g => AutoMapper.Mapper.Map<Job>(g)));
            }

            return _dbContext.JobsByDate
                .Where(j => j.OrganizationId == organizationId && j.Date >= startDate && j.Date <= endDate)
                .GroupBy(j => j.Date)
                .ToDictionary(group => group.Key, group => group.Select(g => AutoMapper.Mapper.Map<Job>(g)));
        }

        public void CopyCalendarDay(Guid organizationId, DateTime dateFrom, DateTime dateTo)
        {
            var dayJobsToDelete = GetDayJobsForDay(dateTo, organizationId);
            foreach (var toDelete in dayJobsToDelete)
            {
                _dbContext.DaysJobs.Remove(toDelete);
            }

            var copyFrom = _dbContext.DaysJobs
                .Include(dj => dj.DayJobTags)
                .Include(dj => dj.DayJobWorkers)
                .Include(dj => dj.Job)
                .Where(dj => dj.Job.OrganizationId == organizationId && dj.Date.Date == dateFrom.Date);

            foreach (var dayJob in copyFrom)
            {
                var newDayJob = new DayJob()
                {
                    Id = Guid.NewGuid(),
                    Date = dateTo,
                    IdJob = dayJob.IdJob
                };

                _dbContext.DaysJobs.Add(newDayJob);

                foreach (var dayJobWorker in dayJob.DayJobWorkers)
                {
                    _dbContext.DaysJobsWorkers.Add(new DayJobWorker()
                    {
                        Id = Guid.NewGuid(),
                        IdDayJob = newDayJob.Id,
                        IdWorker = dayJobWorker.IdWorker
                    });
                }

                foreach (var dayJobTag in dayJob.DayJobTags)
                {
                    _dbContext.DaysJobsTags.Add(new DayJobTag()
                    {
                        Id = Guid.NewGuid(),
                        IdDayJob = newDayJob.Id,
                        IdTag = dayJobTag.IdTag
                    });
                }
            }

            _dbContext.SaveChanges();
        }

        public Job AddJob(AddJobModel addJobModel, Guid organizationId)
        {
            var job = new Job()
            {
                Id = new Guid(addJobModel.Id),
                Name = addJobModel.Name,
                Number = addJobModel.Number,
                Notes = addJobModel.Notes,
                OrganizationId = organizationId
            };

            _dbContext.Jobs.Add(job);

            foreach (var tag in addJobModel.Tags)
                _tagRepository.AddTagToJob(tag.Id, job.Id);

            foreach (var date in addJobModel.JobDays)
            {
                var dayJob = new DayJob()
                {
                    Id = Guid.NewGuid(),
                    IdJob = job.Id,
                    Date = date.Date
                };

                _dbContext.DaysJobs.Add(dayJob);
            }

            _dbContext.SaveChanges();

            return job;
        }

        public JobStartAndEndDate GetJobStartAndEndDate(Guid jobId, Guid organizationId)
        {
            var startDate = GetDayJobsForJob(jobId, organizationId).Min(dayJob => dayJob.Date);
            var endDate = GetDayJobsForJob(jobId, organizationId).Max(dayJob => dayJob.Date);

            return new JobStartAndEndDate()
            {
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public void EditJob(Guid id, AddJobModel addJobModel, Guid organizationId)
        {
            var job = GetJobsByOrganization(organizationId).FirstOrDefault(w => w.Id == id);

            if (job == null)
                throw new ApplicationException("Job not Found");

            job.Name = addJobModel.Name;
            job.Number = addJobModel.Number;
            job.Notes = addJobModel.Notes;

            _dbContext.Jobs.Update(job);

            //Update Tags
            _tagRepository.UpdateTagsForJob(id, addJobModel.Tags);

            var dayJobs = GetDayJobsForJob(id, organizationId);

            //Delete Day Jobs that are not in the new range
            foreach (var dayJob in dayJobs)
            {
                if (addJobModel.JobDays.Any(d => dayJob.Date.Date == d.Date))
                    continue;

                _dbContext.DaysJobs.Remove(dayJob);
            }

            var dayJobDates = dayJobs.Select(dayJob => dayJob.Date.Date);

            //Create Day Jobs for the new range
            foreach (var date in addJobModel.JobDays)
            {
                if (dayJobDates.Contains(date.Date))
                    continue;

                var dayJob = new DayJob()
                {
                    Id = Guid.NewGuid(),
                    IdJob = job.Id,
                    Date = date.Date
                };

                _dbContext.DaysJobs.Add(dayJob);
            }

            _dbContext.SaveChanges();
        }

        public void AddWorkerToJob(Guid idJob, Guid idWorker, Guid organizationId, DateTime? date = null, bool save = true)
        {
            var job = GetJob(idJob, organizationId);

            if (job == null)
                throw new ApplicationException("Unable to Locate Job");

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

            if (save)
                _dbContext.SaveChanges();
        }

        public void DeleteJob(Guid idJob, Guid organizationId)
        {
            var job = GetJob(idJob, organizationId);

            _dbContext.Jobs.Remove(job);

            _dbContext.SaveChanges();
        }

        public void DeleteDayJob( Guid idJob, DateTime date, Guid organizationid)
        {
            var dayJob = _dbContext.DaysJobs.FirstOrDefault(dj => dj.IdJob == idJob && dj.Date.Date == date.Date);

            if (dayJob == null)
                throw new InvalidOperationException("Unable to locate day job");

            _dbContext.DaysJobs.Remove(dayJob);
            _dbContext.SaveChanges();
        }

        public void DeleteJobsFromDay( DateTime date, Guid organizationId)
        {
            var dayJobs = _dbContext.DaysJobs.Include(dj => dj.Job).Where(dj => dj.Job.OrganizationId == organizationId && dj.Date.Date == date.Date);

            foreach (var dj in dayJobs)
                _dbContext.DaysJobs.Remove(dj);

            _dbContext.SaveChanges();
        }

        public void MoveWorkerToAllDaysOnJob(Guid jobId, Guid workerId, DateTime viewDate, Guid organizationId)
        {
            var start = viewDate.StartOfWeek();
            var end = viewDate.EndOfWeek();
            var jobDays = _dbContext.DaysJobs.Where(d => d.IdJob == jobId && d.Date.Date >= start.Date && d.Date.Date <= end.Date);

            foreach (var jobDay in jobDays)
                MoveWorkerToJob(jobId, workerId, jobDay.Date, organizationId, false);

            _dbContext.SaveChanges();
        }

        public void MoveWorkerToAllAvailableDaysOnJob(Guid jobId, Guid workerId, DateTime date, DateTime viewDate, Guid organizationId)
        {
            var start = viewDate.StartOfWeek();
            var end = viewDate.EndOfWeek();

            var jobDays = _dbContext.DaysJobs.Where(d => d.IdJob == jobId && d.Date.Date >= start.Date && d.Date.Date <= end.Date);
            var workingDays = _dbContext.DaysJobsWorkers.Include(djw => djw.DayJob).Where(djw => djw.IdWorker == workerId).Select(djw => djw.DayJob).Where(dj => dj.Date.Date != date.Date );

            var availableDays = jobDays.Where(d => !workingDays.Any(wd => wd.Date.Date == d.Date.Date));

            foreach (var jobDay in availableDays)
                MoveWorkerToJob(jobId, workerId, jobDay.Date, organizationId, false);

            _dbContext.SaveChanges();
        }

        public void MoveWorkerToJob(Guid idJob, Guid idWorker, DateTime date, Guid organizationId, bool save = true)
        {
            //Make into repository method that only includes jobs for the given organization           
            var jobsOnDay = _dbContext.DaysJobs.Include(dj => dj.DayJobWorkers).Where(dj => dj.Date.Date == date.Date);

            var fromJob = jobsOnDay.FirstOrDefault(dj => dj.DayJobWorkers.Any(djw => djw.IdWorker == idWorker));
            var toJob = jobsOnDay.FirstOrDefault(j => j.IdJob == idJob);

            var dayOff = _dbContext.DayOffWorkers.FirstOrDefault(dow => dow.IdWorker == idWorker && dow.Date.Date == date.Date);
            if (dayOff != null)
                _dbContext.DayOffWorkers.Remove(dayOff);

            if (fromJob != null)
            {
                var existingWorkerDay = fromJob.DayJobWorkers.FirstOrDefault(djw => djw.IdWorker == idWorker);

                existingWorkerDay.IdDayJob = toJob.Id;

                if (save)
                    _dbContext.SaveChanges();

                return;
            }

            AddWorkerToJob(idJob, idWorker, organizationId, date, save);
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
                Date = date.Date
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

        public void SaveNotes(Guid idJob, Guid organizationId, string notes)
        {
            var job = GetJob(idJob, organizationId);

            _dbContext.Attach(job);

            job.Notes = notes;

            _dbContext.SaveChanges();
        }

        #endregion        
    }
}
