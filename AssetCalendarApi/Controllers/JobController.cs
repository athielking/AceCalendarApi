using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using AssetCalendarApi.Validators;
using Microsoft.AspNetCore.Identity;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class JobController : ApiBaseController
    {
        #region Data Members

        private readonly JobRepository _jobRepository;

        private readonly WorkerValidator _validator;

        #endregion

        #region Constructor

        public JobController
        (
            JobRepository jobRepository,
            WorkerValidator validator,
            UserManager<CalendarUser> userManager
        ): base(userManager)
        {
            _jobRepository = jobRepository;
            _validator = validator;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                var jobs = _jobRepository.GetAllJobs(calendarUser.OrganizationId).ToList();

                return SuccessResult(jobs);
            }
            catch
            {
                return BadRequest("Failed To Gset Jobs");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                var job = _jobRepository.GetJob(id, calendarUser.OrganizationId);

                if (job == null)
                    return NotFound($"Job with Id {id} not found");

                return SuccessResult(job);
            }
            catch
            {
                return BadRequest("Failed To Get Job");
            }
        }

        [HttpGet]
        [Route("getJobsForDay")]
        public async Task<IActionResult> GetJobsForDay(DateTime date)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                return SuccessResult(_jobRepository.GetJobsForDay(date, calendarUser.OrganizationId));
            }
            catch
            {
                return BadRequest("Failed Get Jobs for Day");
            }
        }

        [HttpGet]
        [Route("getJobsForWeek")]
        public async Task<IActionResult> GetJobsForWeek(DateTime date)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                return SuccessResult(_jobRepository.GetJobsForWeek(date, calendarUser.OrganizationId));
            }
            catch
            {
                return BadRequest("Failed Get Jobs for Week");
            }
        }

        [HttpGet]
        [Route("getJobsForMonth")]
        public async Task<IActionResult> GetJobsForMonth(DateTime date)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                return SuccessResult(_jobRepository.GetJobsForMonth(date, calendarUser.OrganizationId));
            }
            catch
            {
                return BadRequest("Failed Get Jobs for Month");
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]AddJobModel job)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var addedJob = _jobRepository.AddJob(job, CalendarUser.OrganizationId);

                return Ok(addedJob);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Job"));
            }
        }

        [HttpPost("moveWorkerToJob")]
        public IActionResult MoveWorkerToJob([FromBody]MoveWorkerRequestModel model)
        {
            try
            {
                _jobRepository.MoveWorkerToJob(model.IdJob.Value, model.IdWorker, model.Date.Value, CalendarUser.OrganizationId);

                return SuccessResult("Worker Successfully Moved");
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed To Move Worker to Job"));
            }
        }

        [HttpPost("moveWorkerToAvailable")]
        public IActionResult MoveWorkerToAvailable([FromBody]MoveWorkerRequestModel model)
        {
            try
            {
                _jobRepository.MakeWorkerAvailable(model.IdWorker, model.Date.Value, CalendarUser.OrganizationId);

                return SuccessResult("Worker Successfully Moved");
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Move Worker to Available"));
            }
        }

        [HttpPost("moveWorkerToOff")]
        public IActionResult MoveWorkerToOff([FromBody]MoveWorkerRequestModel model)
        {
            try
            {
                _jobRepository.MoveWorkerToOff(model.IdWorker, model.Date.Value, CalendarUser.OrganizationId);

                return SuccessResult("Worker Successfully Moved");
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Move Worker to Off"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _jobRepository.DeleteJob(id, CalendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed To Delete Job"));
            }
        }

        [HttpPost("saveNotes/{id}")]
        public IActionResult SaveNotes(Guid id, [FromBody]SaveNotesRequestViewModel saveNotesRequestViewModel)
        {
            try
            {
                _jobRepository.SaveNotes(id, CalendarUser.OrganizationId, saveNotesRequestViewModel.Notes);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Save Notes"));
            }
        }

        #endregion
    }
}
