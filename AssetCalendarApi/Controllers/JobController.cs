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
        public async Task<IActionResult> Post([FromBody]AddJobModel model)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);
                
                var daysNotAvailable = new Dictionary<Guid, IEnumerable<DateTime>>();

                foreach (var guid in model.WorkerIds)
                {
                    var working = _validator.GetDaysWorking(guid, calendarUser.OrganizationId, model.StartDate, model.EndDate);
                    if (working.Any())
                        daysNotAvailable.Add(guid, working);
                }

                if (daysNotAvailable.Any())
                {
                    return BadRequest(new
                    {
                        message = "Failed to add job: Workers Unavailable",
                        data = daysNotAvailable
                    });
                }

                var job = _jobRepository.AddJob(model, calendarUser.OrganizationId);
                return Ok(job);
            }
            catch
            {
                return BadRequest("Failed to add Job");
            }
        }

        [HttpPost("moveWorkerToJob")]
        public async Task<IActionResult> MoveWorkerToJob([FromBody]MoveWorkerRequestModel model)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                _jobRepository.MoveWorkerToJob(model.IdJob.Value, model.IdWorker, model.Date.Value, calendarUser.OrganizationId);

                return SuccessResult("Worker Successfully Moved");
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed To Move Worker to Job"));
            }
        }

        [HttpPost("moveWorkerToAvailable")]
        public async Task<IActionResult> MoveWorkerToAvailable([FromBody]MoveWorkerRequestModel model)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                _jobRepository.MakeWorkerAvailable(model.IdWorker, model.Date.Value, calendarUser.OrganizationId);

                return SuccessResult("Worker Successfully Moved");
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Move Worker to Available"));
            }
        }

        [HttpPost("moveWorkerToOff")]
        public async Task<IActionResult> MoveWorkerToOff([FromBody]MoveWorkerRequestModel model)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                _jobRepository.MoveWorkerToOff(model.IdWorker, model.Date.Value, calendarUser.OrganizationId);

                return SuccessResult("Worker Successfully Moved");
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Move Worker to Off"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                _jobRepository.DeleteJob(id, calendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed To Delete Job"));
            }
        }

        [HttpPost("saveNotes/{id}")]
        public async Task<IActionResult> SaveNotes(Guid id, [FromBody]SaveNotesRequestViewModel saveNotesRequestViewModel)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                _jobRepository.SaveNotes(id, calendarUser.OrganizationId, saveNotesRequestViewModel.Notes);

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
