using AssetCalendarApi.Models;
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

        public IQueryable<Worker> GetAvailableWorkers( DateTime date )
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
    }
}
