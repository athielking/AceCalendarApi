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

                //Need the calendar to show from sunday to saturday regardless of month
                DateTime monthStart = new DateTime(date.Year, date.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1).EndOfWeek();

                return SuccessResult(GetDataForRange(monthStart, monthEnd, idWorker));
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

                DateTime weekStart = date.StartOfWeek();
                DateTime weekEnd = date.EndOfWeek();
              
                return SuccessResult(GetDataForRange(weekStart, weekEnd, idWorker));
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

                return SuccessResult(GetDataForRange(date, null, idWorker));
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Get Data For Day"));
            }
        }

        [HttpGet]
        [Route("getRange")]
        public IActionResult GetDataForRange( DateTime date, DateTime? endDate, Guid? idWorker)
        {
            var jobsByDate = _jobRepository.GetJobsForRange(CalendarUser.OrganizationId, date, endDate, idWorker);

            var workersByDate = _workerRepository.GetAvailableWorkersForDates(CalendarUser.OrganizationId, date, endDate);
            var offByDate = _workerRepository.GetOffWorkersForDates(CalendarUser.OrganizationId, date, endDate);
            var tagsByWorker = _tagRepository.GetTagsByWorker(CalendarUser.OrganizationId);

            var end = endDate.HasValue ? endDate.Value : date;

            Dictionary<DateTime, DayViewModel> result = new Dictionary<DateTime, DayViewModel>();
            for (DateTime d = date.Date; d <= end.Date; d = d.AddDays(1))
            {
                DayViewModel vm = new DayViewModel()
                {
                    Date = d,
                    AvailableWorkers = (workersByDate.ContainsKey(d) ? workersByDate[d] : Enumerable.Empty<Worker>()).OrderBy(worker => worker.FullName),
                    TimeOffWorkers = (offByDate.ContainsKey(d) ? offByDate[d] : Enumerable.Empty<Worker>()).OrderBy(worker => worker.FullName),
                    Jobs = (jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>()).OrderBy(job => job.Name)
                };
                vm.WorkersByJob = _workerRepository.GetWorkersByJob(d, CalendarUser.OrganizationId);
                vm.TagsByJob = _tagRepository.GetTagsByJob(d, CalendarUser.OrganizationId);
                vm.TagsByWorker = tagsByWorker;
                

                result.Add(d, vm);
            }

            return SuccessResult(result);
        }

        [HttpPost]
        [Route("copyCalendarDay")]
        public IActionResult CopyCalendarDay( DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                _jobRepository.CopyCalendarDay(CalendarUser.OrganizationId, dateFrom, dateTo);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Copy Calendar Day"));
            }
        }
        #endregion
    }
}
