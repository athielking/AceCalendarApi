using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.SignalR;
using AssetCalendarApi.Hubs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class WorkerController : ApiBaseController
    {
        #region Data Members

        private readonly WorkerRepository _workerRepository;
        private readonly IHubContext<CalendarHub> _hubContext;

        #endregion

        #region Constructor

        public WorkerController
        (
            WorkerRepository workerRepository,
            UserManager<AceUser> userManager,
            IHubContext<CalendarHub> hubContext
        ) : base(userManager)
        {
            _workerRepository = workerRepository;
            _hubContext = hubContext;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var workers = _workerRepository.GetAllWorkers(CalendarId)
                    .ToList()
                    .Select(worker => worker.GetViewModel() )
                    .OrderBy(worker => worker.FullName)
                    .ToList();

                return SuccessResult(workers);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Workers"));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var worker = _workerRepository.GetWorker(id, CalendarId);

                return SuccessResult(worker.GetViewModel());
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Worker"));
            }
        }

        [HttpGet]
        [Route("getAvailableWorkers")]
        public IActionResult GetAvailableWorkers(DateTime start, DateTime? end)
        {
            throw new NotImplementedException();
            //try
            //{
            //    //return SuccessResult(_workerRepository.GetAvailableWorkers(CalendarUser.OrganizationId, start, end).ToList());
            //}
            //catch
            //{
            //    return BadRequest(GetErrorMessageObject("Failed To Get Available Workers"));
            //}
        }

        [HttpGet]
        [Route("getTimeOffForMonth")]
        public IActionResult GetTimeOffForMonth(Guid id, DateTime date)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var timeOffForMonth = _workerRepository.GetTimeOffForMonth(id, date, CalendarId).ToList();

                return SuccessResult(timeOffForMonth);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Time Off for Month"));
            }
        }

        [HttpGet]
        [Route("getJobsForMonth")]
        public IActionResult GetJobsForMonth(Guid id, DateTime date)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var jobsForMonth = _workerRepository.GetJobsForMonth(id, date, CalendarId).Select(dayJobWorker =>
                {
                    return new JobByDateModel()
                    {
                        Date = dayJobWorker.DayJob.Date,
                        JobName = dayJobWorker.DayJob.Job.Name,
                        JobNumber = dayJobWorker.DayJob.Job.Number
                    };

                }).ToList();

                return SuccessResult(jobsForMonth);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Jobs for Month"));
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]WorkerViewModel worker)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var addedWorker = _workerRepository.AddWorker(worker, CalendarId);

                return Ok(addedWorker);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Worker"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]WorkerViewModel worker)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _workerRepository.EditWorker(id, worker, CalendarId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Update Worker"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _workerRepository.DeleteWorker(id, CalendarId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Worker"));
            }
        }

        [HttpDelete("deleteTimeOff")]
        public IActionResult DeleteTimeOff(Guid workerId, DateTime date)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _workerRepository.DeleteTimeOff(workerId, date, CalendarId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Time Off"));
            }
        }

        [HttpPost("editTimeOff")]
        public IActionResult EditTimeOff([FromBody]EditTimeOffModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _workerRepository.EditTimeOff(model.WorkerId, model.MonthDate, model.TimeOffDates, CalendarId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Time Off"));
            }
        }

        #endregion
    }
}
