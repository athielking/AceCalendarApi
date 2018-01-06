using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using AssetCalendarApi.Models;

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : ApiBaseController
    {
        private JobRepository _jobRepository;
        private WorkerRepository _workerRepository;

        public CalendarController(JobRepository jobRepository, WorkerRepository workerRepository )
        {
            _jobRepository = jobRepository;
            _workerRepository = workerRepository;
        }

        // GET api/values
        [HttpGet]
        [Route("getMonth")]
        public IActionResult GetDataForMonth(DateTime date)
        {
            if (date.Year < 1900)
                return BadRequest("Invalid Date");

            var jobsByDate = _jobRepository.GetJobsForMonth(date);
            var workersByDate = _workerRepository.GetAvailableWorkersForMonth(date);

            //Need the calendar to show from sunday to saturday regardless of month
            DateTime monthStart = new DateTime(date.Year, date.Month, 1).StartOfWeek();
            DateTime monthEnd = new DateTime(date.Year, date.Month + 1, 1).AddDays(-1).EndOfWeek();

            Dictionary<DateTime, DayViewModel> monthData = new Dictionary<DateTime, DayViewModel>();
            for(DateTime d = monthStart; d <= monthEnd; d = d.AddDays(1))
            {
                DayViewModel vm = new DayViewModel()
                {
                    Date = d,
                    AvailableWorkers = workersByDate.ContainsKey(d) ? workersByDate[d] : Enumerable.Empty<Worker>(),
                    Jobs = jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>()
                };

                monthData.Add(d, vm);
            }

            return SuccessResult(monthData);
        }

        // GET api/values/5
        [HttpGet]
        [Route("getWeek")]
        public IActionResult GetDataForWeek(DateTime date)
        {
            if (date.Year < 1900)
                return BadRequest("Invalid Date");

            var jobsByDate = _jobRepository.GetJobsForWeek(date);
            var workersByDate = _workerRepository.GetAvailableWorkersForWeek(date);

            DateTime monthStart = new DateTime(date.Year, date.Month, 1);
            DateTime monthEnd = new DateTime(date.Year, date.Month + 1, 1).AddDays(-1);

            Dictionary<DateTime, DayViewModel> monthData = new Dictionary<DateTime, DayViewModel>();
            for (DateTime d = date.StartOfWeek(); d <= date.EndOfWeek(); d = d.AddDays(1))
            {
                DayViewModel vm = new DayViewModel()
                {
                    Date = d,
                    AvailableWorkers = workersByDate.ContainsKey(d) ? workersByDate[d] : Enumerable.Empty<Worker>(),
                    Jobs = jobsByDate.ContainsKey(d) ? jobsByDate[d] : Enumerable.Empty<Job>()
                };

                monthData.Add(d, vm);
            }

            return SuccessResult(monthData);
        }

        [HttpGet]
        [Route("getDay")]
        public IActionResult GetDataForDay(DateTime date)
        {
            if (date.Year < 1900)
                return BadRequest("Invalid Date");

            var jobs = _jobRepository.GetJobsForDay(date);
            var workers = _workerRepository.GetAvailableWorkersForDay(date);

            return SuccessResult(
                new DayViewModel()
                {
                    Date = date,
                    AvailableWorkers = workers,
                    Jobs = jobs
                });
        }

    }
}
