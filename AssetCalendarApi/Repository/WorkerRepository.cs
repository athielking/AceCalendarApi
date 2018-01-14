using AssetCalendarApi.Data;
using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Repository
{
    public class WorkerRepository
    {
        private AssetCalendarDbContext _dbContext;

        public WorkerRepository(AssetCalendarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Worker> GetAllWorkers()
        {
            return _dbContext.Workers;
        }

        public Worker GetWorker(Guid id)
        {
            return
                _dbContext.Workers
                    .Include(worker => worker.DayJobWorkers)
                        .ThenInclude(djw => djw.DayJob)
                    .FirstOrDefault(w => w.Id == id);
        }

        public IQueryable<Worker> GetAvailableWorkers(DateTime start, DateTime? end = null)
        {

            if (end.HasValue)
            {
                return
                    _dbContext.Workers.Except(
                      _dbContext.DaysJobs
                          .Where(dj => dj.Date >= start && dj.Date <= end.Value)
                          .Join(_dbContext.DaysJobsWorkers,
                              dj => dj.Id,
                              djw => djw.IdDayJob,
                              (dayJob, dayJobWorker) => dayJobWorker.Worker));
            }

            return
                _dbContext.Workers.Except(
                      _dbContext.DaysJobs
                          .Where(dj => dj.Date == start)
                          .Join(_dbContext.DaysJobsWorkers,
                              dj => dj.Id,
                              djw => djw.IdDayJob,
                              (dayJob, dayJobWorker) => dayJobWorker.Worker));

        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForMonth(DateTime month)
        {
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = new DateTime(month.Year, month.Month + 1, 1).AddDays(-1);

            return GetAvailableWorkersForDates(startOfMonth, endOfMonth);
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForWeek(DateTime week)
        {
            return GetAvailableWorkersForDates(week.StartOfWeek(), week.EndOfWeek());
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForDates(DateTime start, DateTime? end)
        {
            var allWorkers = _dbContext.Workers.AsEnumerable();
            var allDates = start.GetDatesTo(end);

            //Get the cartesian product for all dates + all workers
            var available =
                from worker in allWorkers
                from date in allDates
                select new { date, worker };

            //Get a dictionary of who is working on what day
            var working = _dbContext.DaysJobs.Where(dj => dj.Date >= start && dj.Date <= allDates.Last())
                    .Join(_dbContext.DaysJobsWorkers,
                        dj => dj.Id,
                        djw => djw.IdDayJob,
                        (dayJob, dayJobWorker) => new { dayJob.Date, dayJobWorker.IdWorker })
                    .GroupBy(m => m.Date)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(m => m.IdWorker));

            //Get a dictionary of dates, and who is not already working
            var availableWorkers = available
                .GroupBy(m => m.date)
                .ToDictionary(
                    group => group.Key,
                    group => group.Where(m => !working.ContainsKey(group.Key) || (working.ContainsKey(group.Key) && !working[group.Key].Contains(m.worker.Id)))
                                  .Select(m => m.worker));

            return availableWorkers;
        }

        public IQueryable<Worker> GetWorkersForJob(Guid idJob)
        {
            return
                _dbContext.Workers
                    .Join(_dbContext.DaysJobsWorkers,
                        w => w.Id,
                        d => d.IdWorker,
                        (worker, d) => new { d.IdDayJob, worker })
                    .Join(_dbContext.DaysJobs,
                        m => m.IdDayJob,
                        j => j.IdJob,
                        (m, j) => new { j.IdJob, m.worker })
                    .Where(x => x.IdJob == idJob)
                    .Select(x => x.worker);
        }

        public WorkerViewModel AddWorker(WorkerViewModel worker)
        {
            var dbWorker = new Worker()
            {
                Id = Guid.NewGuid(),
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Email = worker.Email,
                Phone = worker.Phone
            };

            _dbContext.Workers.Add(dbWorker);
            _dbContext.SaveChanges();

            worker.Id = dbWorker.Id.ToString();

            return worker;
        }

        public void DeleteWorker(string id)
        {
            var guid = new Guid(id);

            var worker = _dbContext.Workers.FirstOrDefault(w => w.Id == guid);
            if (worker == null)
                throw new Exception("Worker not Found");

            _dbContext.Workers.Remove(worker);
            _dbContext.SaveChanges();
        }
    }
}
