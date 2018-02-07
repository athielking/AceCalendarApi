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

        private readonly UserManager<CalendarUser> _userManager;

        private readonly WorkerRepository _workerRepository;

        #endregion

        #region Constructor

        public CalendarController
        (
            JobRepository jobRepository,
            WorkerRepository workerRepository,
            UserManager<CalendarUser> userManager
        )
        {
            _jobRepository = jobRepository;
            _workerRepository = workerRepository;
            _userManager = userManager;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        [Route("getMonth")]
        public async Task<IActionResult> GetDataForMonth(DateTime date)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                var jobsByDate = _jobRepository.GetJobsForMonth(date, calendarUser.OrganizationId);
                var workersByDate = _workerRepository.GetAvailableWorkersForMonth(calendarUser.OrganizationId, date);

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
                        Jobs = jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>()
                    };

                    vm.WorkersByJob = vm.Jobs
                        .Select(j => new
                        {
                            id = j.Id,
                            workers = _workerRepository.GetWorkersForJob(j.Id, d, calendarUser.OrganizationId)
                        })
                        .GroupBy(m => m.id)
                        .ToDictionary(group => group.Key, group => group.SelectMany(g => g.workers));

                    monthData.Add(d, vm);
                }

                return SuccessResult(monthData);
            }
            catch
            {
                return BadRequest("Failed To Get Data For Month");
            }
        }

        [HttpGet]
        [Route("getWeek")]
        public async Task<IActionResult> GetDataForWeek(DateTime date)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                var jobsByDate = _jobRepository.GetJobsForWeek(date, calendarUser.OrganizationId);
                var workersByDate = _workerRepository.GetAvailableWorkersForWeek(calendarUser.OrganizationId, date);

                Dictionary<DateTime, DayViewModel> monthData = new Dictionary<DateTime, DayViewModel>();
                for (DateTime d = date.StartOfWeek(); d <= date.EndOfWeek(); d = d.AddDays(1))
                {
                    DayViewModel vm = new DayViewModel()
                    {
                        Date = d,
                        AvailableWorkers = workersByDate.ContainsKey(d) ? workersByDate[d] : Enumerable.Empty<Worker>(),
                        Jobs = jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>()
                    };
                    vm.WorkersByJob = vm.Jobs
                        .Select(j => new
                        {
                            id = j.Id,
                            workers = _workerRepository.GetWorkersForJob(j.Id, d, calendarUser.OrganizationId).ToArray()
                        })
                        .GroupBy(m => m.id)
                        .ToDictionary(group => group.Key, group => group.SelectMany(g => g.workers));

                    monthData.Add(d, vm);
                }

                return SuccessResult(monthData);
            }
            catch
            {
                return BadRequest("Get Data For Week");
            }
        }

        [HttpGet]
        [Route("getDay")]
        public async Task<IActionResult> GetDataForDay(DateTime date)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                var jobs = _jobRepository.GetJobsForDay(date, calendarUser.OrganizationId);
                var workers = _workerRepository.GetAvailableWorkers(calendarUser.OrganizationId, date);
                var workersByJob = jobs
                        .Select(j => new
                        {
                            id = j.Id,
                            workers = _workerRepository.GetWorkersForJob(j.Id, date, calendarUser.OrganizationId)
                        })
                        .GroupBy(m => m.id)
                        .ToDictionary(group => group.Key, group => group.SelectMany(g => g.workers));

                var dayData = new Dictionary<DateTime, DayViewModel>();
                dayData.Add(date, new DayViewModel()
                {
                    Date = date,
                    AvailableWorkers = workers,
                    Jobs = jobs,
                    WorkersByJob = workersByJob
                });

                return SuccessResult(dayData);
            }
            catch
            {
                return BadRequest("Get Data For Day");
            }
        }

        #endregion
    }
}
