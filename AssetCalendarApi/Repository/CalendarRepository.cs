using AssetCalendarApi.Data.Models;
using AssetCalendarApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Repository
{
    public class CalendarRepository
    {
        private readonly WorkerRepository _workerRepository;
        private readonly JobRepository _jobRepository;
        private readonly TagRepository _tagRepository;

        public CalendarRepository(WorkerRepository workerRepository, JobRepository jobRepository, TagRepository tagRepository)
        {
            _workerRepository = workerRepository;
            _jobRepository = jobRepository;
            _tagRepository = tagRepository;
        }

        public Dictionary<DateTime, DayViewModel> GetDataForRange(DateTime date, Guid organizationId, DateTime? endDate = null, Guid? idWorker = null)
        {
            var availableByDate = _workerRepository.GetAvailableWorkersForDates(organizationId, date, endDate);
            var offByDate = _workerRepository.GetOffWorkersForDates(organizationId, date, endDate);

            var jobsByDate = _jobRepository.GetJobsForRange(organizationId, date, endDate, idWorker);
            var tagsByWorker = _tagRepository.GetTagsByWorker(organizationId);

            var end = endDate.HasValue ? endDate.Value : date;

            Dictionary<DateTime, DayViewModel> result = new Dictionary<DateTime, DayViewModel>();
            for (DateTime d = date.Date; d <= end.Date; d = d.AddDays(1))
            {
                DayViewModel vm = new DayViewModel()
                {
                    Date = d,
                    AvailableWorkers = availableByDate.ContainsKey(d) ? availableByDate[d] : Enumerable.Empty<Worker>(),
                    TimeOffWorkers = offByDate.ContainsKey(d) ? offByDate[d] : Enumerable.Empty<Worker>(),
                    Jobs = jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>()
                };
                vm.WorkersByJob = _workerRepository.GetWorkersByJob(d, organizationId);
                vm.TagsByJob = _tagRepository.GetTagsByJob(d, organizationId);
                vm.TagsByWorker = tagsByWorker;

                result.Add(d, vm);
            }

            return result;
        }

        public Task<Dictionary<DateTime, DayViewModel>>GetDataForRangeAsync(DateTime date, Guid organizationId, DateTime? endDate, Guid? idWorker)
        {
            return Task<Dictionary<DateTime, DayViewModel>>.Run(() =>
            {
                return GetDataForRange(date, organizationId, endDate, idWorker);
            });
        }
    }
}
