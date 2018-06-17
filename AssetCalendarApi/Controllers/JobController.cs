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
using Microsoft.AspNetCore.SignalR;
using AssetCalendarApi.Hubs;
using Microsoft.Extensions.DependencyInjection;
using AssetCalendarApi.Tools;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class JobController : ApiBaseController
    {
        #region Data Members

        private readonly JobRepository _jobRepository;
        private readonly TagRepository _tagRepository;
        private readonly SignalRService _signalRService;

        #endregion

        #region Constructor

        public JobController
        (
            JobRepository jobRepository,
            TagRepository tagRepository,
            SignalRService signalRService,
            UserManager<CalendarUser> userManager
        ) : base(userManager)
        {
            _jobRepository = jobRepository;
            _tagRepository = tagRepository;
            _signalRService = signalRService;
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

        [HttpGet]
        [Route("getJobStartAndEndDate")]
        public IActionResult GetJobStartAndEndDate(Guid jobId)
        {
            try
            {
                return SuccessResult(_jobRepository.GetJobStartAndEndDate(jobId, CalendarUser.OrganizationId));
            }
            catch
            {
                return BadRequest("Failed to Get Job Start and End Date");
            }
        }

        [HttpGet]
        [Route("getJobDays")]
        public IActionResult GetJobDays(Guid jobId)
        {
            try
            {
                return SuccessResult(_jobRepository.GetJobDaysForJob(jobId, CalendarUser.OrganizationId).Select( j => j.Date.Date));
            }
            catch
            {
                return BadRequest("Failed to Get Job Start and End Date");
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

                foreach (var jd in addedJob.DaysJobs)
                    _signalRService.SendDataUpdatedAsync(jd.Date, CalendarUser.OrganizationId);

                return SuccessResult(addedJob);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Job"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]AddJobModel job)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                _jobRepository.EditJob(id, job, CalendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Update Job"));
            }
        }

        [HttpPost("moveWorkerToJob")]
        public IActionResult MoveWorkerToJob([FromBody]MoveWorkerRequestModel model)
        {
            try
            {
                if (model.AddWorkerOption == AddWorkerOption.AllDays)
                    _jobRepository.MoveWorkerToAllDaysOnJob(model.IdJob.Value, model.IdWorker, model.ViewDate.Value, CalendarUser.OrganizationId);
                else if (model.AddWorkerOption == AddWorkerOption.AvailableDays)
                    _jobRepository.MoveWorkerToAllAvailableDaysOnJob(model.IdJob.Value, model.IdWorker, model.Date.Value, model.ViewDate.Value, CalendarUser.OrganizationId);
                else
                {
                    _jobRepository.MoveWorkerToJob(model.IdJob.Value, model.IdWorker, model.Date.Value, CalendarUser.OrganizationId);
                    _signalRService.SendDataUpdatedAsync(model.Date.Value, CalendarUser.OrganizationId);
                }

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
                _signalRService.SendDataUpdatedAsync(model.Date.Value, CalendarUser.OrganizationId);
                
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
                _signalRService.SendDataUpdatedAsync(model.Date.Value, CalendarUser.OrganizationId);

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

        [HttpDelete("deleteDayJob/{id}/{date}")]
        public IActionResult DeleteDayJob(Guid id, DateTime date)
        {
            try
            {
                _jobRepository.DeleteDayJob(id, date, CalendarUser.OrganizationId);
                _signalRService.SendDataUpdatedAsync(date, CalendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Job Day"));
            }
        }

        [HttpDelete("deleteJobs/{date}")]
        public IActionResult DeleteJobsFromDay(DateTime date)
        {
            try
            {
                _jobRepository.DeleteJobsFromDay(date, CalendarUser.OrganizationId);
                _signalRService.SendDataUpdatedAsync(date, CalendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Jobs from Day"));
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

        [HttpPost("saveTags/{id}")]
        public IActionResult SaveTags(Guid id, [FromBody]SaveTagsRequestViewModel saveTagsRequest)
        {
            try
            {
                if (saveTagsRequest.Date.HasValue)
                    _tagRepository.DeleteTagsFromJobDay(id, saveTagsRequest.Date.Value);
                else
                    _tagRepository.DeleteTagsFromJob(id);

                foreach(var tag in saveTagsRequest.Tags)
                {
                    if (saveTagsRequest.Date.HasValue)
                        _tagRepository.AddTagToJobDay(tag.Id, id, saveTagsRequest.Date.Value);
                    else
                        _tagRepository.AddTagToJob(tag.Id, id);
                }

                if (saveTagsRequest.Date.HasValue)
                    _signalRService.SendDataUpdatedAsync(saveTagsRequest.Date.Value, CalendarUser.OrganizationId);

                    return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Save Tags"));
            }
        }

        #endregion
    }
}
