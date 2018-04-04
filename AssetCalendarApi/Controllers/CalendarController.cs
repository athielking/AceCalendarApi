using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : ApiBaseController
    {
        #region Data Members

        private readonly JobRepository _jobRepository;

        private readonly WorkerRepository _workerRepository;

        private readonly TagRepository _tagRepository;
        #endregion

        #region Constructor

        public CalendarController
        (
            JobRepository jobRepository,
            WorkerRepository workerRepository,
            TagRepository tagRepository,
            UserManager<CalendarUser> userManager
        ): base(userManager)
        {
            _jobRepository = jobRepository;
            _workerRepository = workerRepository;
            _tagRepository = tagRepository;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        [Route("getMonth")]
        public IActionResult GetDataForMonth(DateTime date, Guid? idWorker)
        {
            try
            {
                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                var jobsByDate = idWorker == null ?
                    _jobRepository.GetJobsForMonth(date, CalendarUser.OrganizationId) :
                    _jobRepository.GetJobsForMonthByWorker(date, CalendarUser.OrganizationId, idWorker.Value);

                var workersByDate = _workerRepository.GetAvailableWorkersForMonth(CalendarUser.OrganizationId, date);
                var timeOffWorkers = _workerRepository.GetOffWorkersForMonth(CalendarUser.OrganizationId, date);

                //Need the calendar to show from sunday to saturday regardless of month
                DateTime monthStart = new DateTime(date.Year, date.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1).EndOfWeek();

                Dictionary<DateTime, DayViewModel> monthData = new Dictionary<DateTime, DayViewModel>();
                for (DateTime d = monthStart.StartOfWeek(); d <= monthEnd; d = d.AddDays(1))
                {
                    DayViewModel vm = new DayViewModel()
                    {
                        Date = d,
                        AvailableWorkers = workersByDate.ContainsKey(d) ? workersByDate[d] : Enumerable.Empty<Worker>(),
                        Jobs = jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>(),
                        TimeOffWorkers = timeOffWorkers.ContainsKey(d) ? timeOffWorkers[d] : Enumerable.Empty<Worker>()
                    };

                    vm.WorkersByJob = _workerRepository.GetWorkersByJob(d, CalendarUser.OrganizationId);
                    vm.TagsByJob = _tagRepository.GetTagsByJob(d, CalendarUser.OrganizationId);

                    monthData.Add(d, vm);
                }

                return SuccessResult(monthData);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Data for Month"));
            }
        }

        [HttpGet]
        [Route("getWeek")]
        public IActionResult GetDataForWeek(DateTime date, Guid? idWorker)
        {
            try
            {
                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                var jobsByDate = idWorker == null ?
                    _jobRepository.GetJobsForWeek(date, CalendarUser.OrganizationId) :
                    _jobRepository.GetJobsForWeekByWorker(date, CalendarUser.OrganizationId, idWorker.Value);

                var workersByDate = _workerRepository.GetAvailableWorkersForWeek(CalendarUser.OrganizationId, date);
                var offByDate = _workerRepository.GetOffWorkersForWeek(CalendarUser.OrganizationId, date);

                Dictionary<DateTime, DayViewModel> monthData = new Dictionary<DateTime, DayViewModel>();
                for (DateTime d = date.StartOfWeek(); d <= date.EndOfWeek(); d = d.AddDays(1))
                {
                    DayViewModel vm = new DayViewModel()
                    {
                        Date = d,
                        AvailableWorkers = ( workersByDate.ContainsKey(d) ? workersByDate[d] : Enumerable.Empty<Worker>() ).OrderBy(worker => worker.FullName),
                        TimeOffWorkers = ( offByDate.ContainsKey(d) ? offByDate[d] : Enumerable.Empty<Worker>() ).OrderBy(worker => worker.FullName),
                        Jobs = ( jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>() ).OrderBy( job => job.Name )
                    };
                    vm.WorkersByJob = _workerRepository.GetWorkersByJob(d, CalendarUser.OrganizationId);
                    vm.TagsByJob = _tagRepository.GetTagsByJob(d, CalendarUser.OrganizationId);

                    monthData.Add(d, vm);
                }

                return SuccessResult(monthData);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Data for Week"));
            }
        }

        [HttpGet]
        [Route("getDay")]
        public IActionResult GetDataForDay(DateTime date, Guid? idWorker)
        {
            try
            {
                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                var jobs = idWorker == null ? 
                    _jobRepository.GetJobsForDay(date, CalendarUser.OrganizationId) : 
                    _jobRepository.GetJobsForDayByWorker(date, CalendarUser.OrganizationId, idWorker.Value);

                var workers = _workerRepository.GetAvailableWorkersForDay(CalendarUser.OrganizationId, date);
                var offByDate = _workerRepository.GetOffWorkersForDates(CalendarUser.OrganizationId, date, date);
                var workersByJob = _workerRepository.GetWorkersByJob(date, CalendarUser.OrganizationId);
                var tagsByJob = _tagRepository.GetTagsByJob(date, CalendarUser.OrganizationId);
                var dayData = new Dictionary<DateTime, DayViewModel>();
                dayData.Add(date, new DayViewModel()
                {
                    Date = date,
                    AvailableWorkers = workers,
                    Jobs = jobs,
                    WorkersByJob = workersByJob,
                    TagsByJob = tagsByJob,
                    TimeOffWorkers = (offByDate.ContainsKey(date.Date) ? offByDate[date.Date] : Enumerable.Empty<Worker>()).OrderBy(worker => worker.FullName),
                });

                return SuccessResult(dayData);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Get Data For Day"));
            }
        }

        [HttpPost]
        [Route("copyCalendarDay")]
        public IActionResult CopyCalendarDay( DateTime dateFrom, DateTime dateTo)
        {
            _jobRepository.CopyCalendarDay(CalendarUser.OrganizationId, dateFrom, dateTo);

            return GetDataForWeek(dateFrom, null);
        }
        #endregion
    }
}
