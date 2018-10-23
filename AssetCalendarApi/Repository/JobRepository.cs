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

        private IQueryable<Job> GetJobsByCalendar(Guid calendarId)
        {
            return _dbContext.Jobs
                .Where(job => job.CalendarId == calendarId);
        }

        private IQueryable<DayJob> GetDayJobsForJob(Guid jobId, Guid calendarId)
        {
            return GetJobsByCalendar(calendarId)
                .AsExpandable()
                .Where(job => job.Id == jobId)
                .Include(job => job.DaysJobs)
                .SelectMany(job => job.DaysJobs);
        }

        #endregion

        #region Public Methods

        public IQueryable<Job> GetAllJobs(Guid calendarId)
        {
            return GetJobsByCalendar(calendarId);
        }

        public Job GetJob(Guid id, Guid calendarId)
        {
            return GetJobsByCalendar(calendarId)
                .AsExpandable()
                .FirstOrDefault(job => job.Id == id);
        }

        public IEnumerable<DayJob> GetJobDaysForJob(Guid jobId, Guid calendarId)
        {
            return _dbContext.DaysJobs.Where(dj => dj.IdJob == jobId);
        }

        public IQueryable<DayJob> GetDayJobsForDay(DateTime date, Guid calendarId)
        {
            return GetJobsByCalendar(calendarId)
                .Include(j => j.DaysJobs)
                .SelectMany(j => j.DaysJobs)
                .Where(dj => dj.Date.Date == date.Date);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForMonth(DateTime month, Guid calendarId, Guid? idWorker = null)
        {
            //Need the calendar to show from sunday to saturday regardless of month
            DateTime monthStart = new DateTime(month.Year, month.Month, 1).StartOfWeek();
            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1).EndOfWeek();

            return GetJobsForRange(calendarId, monthStart, monthEnd, idWorker);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForWeek(DateTime week, Guid calendarId, Guid? idWorker = null)
        {
            return GetJobsForRange(calendarId, week.StartOfWeek(), week.EndOfWeek(), idWorker);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForDay(DateTime date, Guid calendarId, Guid? idWorker = null)
        {
            return GetJobsForRange(calendarId, date, null, idWorker);
        }

        public Dictionary<DateTime, IEnumerable<Job>> GetJobsForRange( Guid calendarId, DateTime start, DateTime? end, Guid? idWorker = null)
        {
            var startDate = start.Date;
            var endDate = end.HasValue ? end.Value.Date : startDate;

            var jobs = _dbContext.DaysJobs.Include(d => d.Job)
                .Where(d => d.Date >= startDate && d.Date <= endDate && d.Job.CalendarId == calendarId);

            if (idWorker.HasValue)
            {
                jobs = _dbContext.DaysJobsWorkers
                    .Include(w => w.DayJob)
                    .ThenInclude(d => d.Job)
                    .Where(w => 
                        w.IdWorker == idWorker &&
                        w.DayJob.Date >= startDate && w.DayJob.Date <= endDate &&
                        w.DayJob.Job.CalendarId == calendarId)
                    .Select(w => w.DayJob);
            }

            var dictionary = new Dictionary<DateTime, IEnumerable<Job>>();
            var keys = jobs.Select(w => w.Date).Distinct();

            foreach (var k in keys)
            {
                dictionary.Add(k, jobs.Where(j => j.Date == k).Select(j => j.Job));
            }

            return dictionary;
        }

        public void CopyCalendarDay(Guid calendarId, DateTime dateFrom, DateTime dateTo)
        {
            var dayJobsToDelete = GetDayJobsForDay(dateTo, calendarId);
            foreach (var toDelete in dayJobsToDelete)
            {
                _dbContext.DaysJobs.Remove(toDelete);
            }

            var copyFrom = _dbContext.DaysJobs
                .Include(dj => dj.DayJobTags)
                .Include(dj => dj.DayJobWorkers)
                .Include(dj => dj.Job)
                .Where(dj => dj.Job.CalendarId == calendarId && dj.Date.Date == dateFrom.Date);

            foreach (var dayJob in copyFrom)
            {
                var newDayJob = new DayJob()
                {
                    Id = Guid.NewGuid(),
                    Date = dateTo.Date,
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

        public Job AddJob(AddJobModel addJobModel, Guid calendarId)
        {
            var job = new Job()
            {
                Id = new Guid(addJobModel.Id),
                Name = addJobModel.Name,
                Number = addJobModel.Number,
                Notes = addJobModel.Notes,
                CalendarId = calendarId
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

        public JobStartAndEndDate GetJobStartAndEndDate(Guid jobId, Guid calendarId)
        {
            var startDate = GetDayJobsForJob(jobId, calendarId).Min(dayJob => dayJob.Date);
            var endDate = GetDayJobsForJob(jobId, calendarId).Max(dayJob => dayJob.Date);

            return new JobStartAndEndDate()
            {
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public void EditJob(Guid id, AddJobModel addJobModel, Guid calendarId)
        {
            var job = GetJobsByCalendar(calendarId).FirstOrDefault(w => w.Id == id);

            if (job == null)
                throw new ApplicationException("Job not Found");

            job.Name = addJobModel.Name;
            job.Number = addJobModel.Number;
            job.Notes = addJobModel.Notes;

            _dbContext.Jobs.Update(job);

            //Update Tags
            _tagRepository.UpdateTagsForJob(id, addJobModel.Tags);

            var dayJobs = GetDayJobsForJob(id, calendarId);

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

        public void AddWorkerToJob(Guid idJob, Guid idWorker, Guid calendarId, DateTime? date = null, bool save = true)
        {
            var job = GetJob(idJob, calendarId);

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

        public void DeleteJob(Guid idJob, Guid calendarId)
        {
            var job = GetJob(idJob, calendarId);

            _dbContext.Jobs.Remove(job);

            _dbContext.SaveChanges();
        }

        public void DeleteDayJob( Guid idJob, DateTime date, Guid calendarId)
        {
            var dayJob = _dbContext.DaysJobs.FirstOrDefault(dj => dj.IdJob == idJob && dj.Date.Date == date.Date);

            if (dayJob == null)
                throw new InvalidOperationException("Unable to locate day job");

            _dbContext.DaysJobs.Remove(dayJob);
            _dbContext.SaveChanges();
        }

        public void DeleteJobsFromDay( DateTime date, Guid calendarId)
        {
            var dayJobs = _dbContext.DaysJobs.Include(dj => dj.Job).Where(dj => dj.Job.CalendarId == calendarId && dj.Date.Date == date.Date);

            foreach (var dj in dayJobs)
                _dbContext.DaysJobs.Remove(dj);

            _dbContext.SaveChanges();
        }

        public void MoveWorkerToAllDaysOnJob(Guid jobId, Guid workerId, DateTime viewDate, Guid calendarId)
        {
            var start = viewDate.StartOfWeek();
            var end = viewDate.EndOfWeek();
            var jobDays = _dbContext.DaysJobs.Where(d => d.IdJob == jobId && d.Date.Date >= start.Date && d.Date.Date <= end.Date);
            var offDays = _dbContext.DayOffWorkers.Where(d => d.IdWorker == workerId && d.Date.Date >= start.Date && d.Date.Date <= end.Date);

            var daysToMove = jobDays.Where(d => !offDays.Any(od => od.Date.Date == d.Date.Date));

            foreach (var jobDay in daysToMove)
                MoveWorkerToJob(jobId, workerId, jobDay.Date, calendarId, false);

            _dbContext.SaveChanges();
        }

        public void MoveWorkerToAllAvailableDaysOnJob(Guid jobId, Guid workerId, DateTime date, DateTime viewDate, Guid calendarId)
        {
            var start = viewDate.StartOfWeek();
            var end = viewDate.EndOfWeek();

            var jobDays = _dbContext.DaysJobs.Where(d => d.IdJob == jobId && d.Date.Date >= start.Date && d.Date.Date <= end.Date);
            var workingDays = _dbContext.DaysJobsWorkers.Include(djw => djw.DayJob).Where(djw => djw.IdWorker == workerId).Select(djw => djw.DayJob).Where(dj => dj.Date.Date != date.Date );

            var availableDays = jobDays.Where(d => !workingDays.Any(wd => wd.Date.Date == d.Date.Date));

            foreach (var jobDay in availableDays)
                MoveWorkerToJob(jobId, workerId, jobDay.Date, calendarId, false);

            _dbContext.SaveChanges();
        }

        public void MoveWorkerToJob(Guid idJob, Guid idWorker, DateTime date, Guid calendarId, bool save = true)
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

            AddWorkerToJob(idJob, idWorker, calendarId, date, save);
        }

        public void MoveWorkerToOff(Guid idWorker, DateTime date, Guid calendarId)
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

        public void MakeWorkerAvailable(Guid idWorker, DateTime date, Guid calendarId)
        {
            var jobWorker = GetDayJobsForDay(date, calendarId)
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

        public void SaveNotes(Guid idJob, Guid calendarId, string notes)
        {
            var job = GetJob(idJob, calendarId);

            _dbContext.Attach(job);

            job.Notes = notes;

            _dbContext.SaveChanges();
        }

        #endregion        
    }
}
