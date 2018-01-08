using AssetCalendarApi.Models;
using AssetCalendarApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Repository
{
    public class WorkerRepository
    {
        private AssetCalendarDbContext _dbContext;

        public WorkerRepository( AssetCalendarDbContext dbContext )
        {
            _dbContext = dbContext;
        }

        public IQueryable<Worker> GetAllWorkers()
        {
            return _dbContext.Workers;
        }

        public Worker GetWorker( Guid id )
        {
            return _dbContext.Workers.FirstOrDefault(w => w.Id == id);
        }

        public IQueryable<Worker> GetAvailableWorkersForDay( DateTime date )
        {
            return
                _dbContext.Workers.Except(
                    _dbContext.DaysJobs
                        .Where(dj => dj.Date == date)
                        .Join( _dbContext.DaysJobsWorkers,
                            dj => dj.Id,
                            djw => djw.IdDayJob,
                            (dayJob, dayJobWorker) => dayJobWorker.Worker)
                );
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForMonth(DateTime month)
        {
            var allWorkers = _dbContext.Workers.AsEnumerable();
            var allDates = month.GetDatesInMonth();

            //Get the cartesian product for all dates + all workers
            var available =
                from worker in allWorkers
                from date in allDates
                select new { date, worker };

            //Get a dictionary of who is working on what day
            var working = _dbContext.DaysJobs
                    .Where(dj => dj.Date.Month == month.Month)
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

        public void DeleteWorker(string id)
        {
            var guid = new Guid(id);

            var worker = _dbContext.Workers.FirstOrDefault(w => w.Id == guid);
            if (worker == null)
                throw new Exception("Worker not Found");

            _dbContext.Workers.Remove(worker);
            _dbContext.SaveChanges();
        }

        public Dictionary<DateTime, IEnumerable<Worker>> GetAvailableWorkersForWeek(DateTime week)
        {
            return
                _dbContext.DaysJobs
                    .Where(dj => dj.Date >= week.StartOfWeek() && dj.Date <= week.EndOfWeek())
                    .Join(_dbContext.DaysJobsWorkers,
                        dj => dj.Id,
                        djw => djw.IdDayJob,
                        (dayJob, dayJobWorker) => new { dayJob.Date, dayJobWorker.Worker })
                    .GroupBy(m => m.Date)
                    .ToDictionary(
                        group => group.Key,
                        group => _dbContext.Workers.Except(group.Select(m => m.Worker)).AsEnumerable());
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
    }
}
