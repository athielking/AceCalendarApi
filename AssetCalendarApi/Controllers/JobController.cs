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
            UserManager<AceUser> userManager
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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                return SuccessResult(_jobRepository.GetAllJobs(CalendarId).ToList());
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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                var job = _jobRepository.GetJob(id, CalendarId);

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
        public IActionResult GetJobsForDay(DateTime date)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                return SuccessResult(_jobRepository.GetJobsForDay(date, CalendarId));
            }
            catch
            {
                return BadRequest("Failed Get Jobs for Day");
            }
        }

        [HttpGet]
        [Route("getJobsForWeek")]
        public IActionResult GetJobsForWeek(DateTime date)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                return SuccessResult(_jobRepository.GetJobsForWeek(date, CalendarId));
            }
            catch
            {
                return BadRequest("Failed Get Jobs for Week");
            }
        }

        [HttpGet]
        [Route("getJobsForMonth")]
        public IActionResult GetJobsForMonth(DateTime date)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                return SuccessResult(_jobRepository.GetJobsForMonth(date, CalendarId));
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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                return SuccessResult(_jobRepository.GetJobStartAndEndDate(jobId, CalendarId));
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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                return SuccessResult(_jobRepository.GetJobDaysForJob(jobId, CalendarId).Select( j => j.Date.Date));
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

                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to Add Job. Calendar Id not set"));

                var addedJob = _jobRepository.AddJob(job, CalendarId);

                foreach (var jd in addedJob.DaysJobs)
                    _signalRService.SendDataUpdatedAsync(jd.Date, CalendarId);

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

                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _jobRepository.EditJob(id, job, CalendarId);
                _signalRService.SendDataUpdatedAsync(job.JobDays.Select( d => d.Date.Date ), CalendarId);

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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                if (model.AddWorkerOption == AddWorkerOption.AllDays)
                    _jobRepository.MoveWorkerToAllDaysOnJob(model.IdJob.Value, model.IdWorker, model.ViewDate.Value, CalendarId);
                else if (model.AddWorkerOption == AddWorkerOption.AvailableDays)
                {
                    _jobRepository.MoveWorkerToAllAvailableDaysOnJob(model.IdJob.Value, model.IdWorker, model.Date.Value, model.ViewDate.Value, CalendarId);
                    _signalRService.SendDataUpdatedAsync(model.ViewDate.Value.StartOfWeek(), CalendarId, model.ViewDate.Value.EndOfWeek());
                }
                else
                {
                    _jobRepository.MoveWorkerToJob(model.IdJob.Value, model.IdWorker, model.Date.Value, CalendarId);
                    _signalRService.SendDataUpdatedAsync(model.Date.Value, CalendarId);
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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _jobRepository.MakeWorkerAvailable(model.IdWorker, model.Date.Value, CalendarId);
                _signalRService.SendDataUpdatedAsync(model.Date.Value, CalendarId);
                
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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _jobRepository.MoveWorkerToOff(model.IdWorker, model.Date.Value, CalendarId);
                _signalRService.SendDataUpdatedAsync(model.Date.Value, CalendarId);

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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _jobRepository.DeleteJob(id, CalendarId);
                _signalRService.SendJobUpdatedAsync(id, CalendarId);

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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _jobRepository.DeleteDayJob(id, date, CalendarId);
                _signalRService.SendDataUpdatedAsync(date, CalendarId);

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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _jobRepository.DeleteJobsFromDay(date, CalendarId);
                _signalRService.SendDataUpdatedAsync(date, CalendarId);

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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _jobRepository.SaveNotes(id, CalendarId, saveNotesRequestViewModel.Notes);
                _signalRService.SendJobUpdatedAsync(id, CalendarId);

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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to Save Tags. Calendar Id not set"));

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
                    _signalRService.SendDataUpdatedAsync(saveTagsRequest.Date.Value, CalendarId);
                else
                    _signalRService.SendJobUpdatedAsync(id, CalendarId);
              
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
