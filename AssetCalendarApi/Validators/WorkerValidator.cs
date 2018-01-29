using AssetCalendarApi.Data.Models;
using AssetCalendarApi.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Validators
{
    public class WorkerValidator
    {
        #region Data Members

        private WorkerRepository _repository;

        #endregion
        
        #region Constructor

        public WorkerValidator(WorkerRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region Public Methods

        public IEnumerable<DateTime> GetDaysWorking(Guid id, Guid organizationId, DateTime startDate, DateTime? endDate)
        {
            var worker = _repository.GetWorkerWithJobs(id, organizationId);
            return GetDaysWorking(worker, startDate, endDate);
        }

        public IEnumerable<DateTime> GetDaysWorking(Worker worker, DateTime startDate, DateTime? endDate)
        {
            if (endDate.HasValue)
            {
                return worker.DayJobWorkers
                     .Where(djw =>
                         djw.DayJob.Date >= startDate && djw.DayJob.Date <= endDate.Value)
                     .Select(djw => djw.DayJob.Date);
            }

            return worker.DayJobWorkers.Where(djw => djw.DayJob.Date == startDate).Select(djw => djw.DayJob.Date);

        }

        public bool IsAvailable(Guid id, Guid organizationId, DateTime startDate, DateTime? endDate)
        {
            var worker = _repository.GetWorkerWithJobs(id, organizationId);
            return IsAvailable(worker, startDate, endDate);
        }

        public bool IsAvailable(Worker worker, DateTime startDate, DateTime? endDate)
        {
            return !worker.DayJobWorkers.Any(djw =>
               djw.DayJob.Date >= startDate &&
               (endDate.HasValue ? djw.DayJob.Date <= endDate.Value : true));

        } 
        
        #endregion
    }
}
