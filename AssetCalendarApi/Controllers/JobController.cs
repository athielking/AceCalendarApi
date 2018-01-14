using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using AssetCalendarApi.Validators;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class JobController : ApiBaseController
    {
        private JobRepository _jobRepository;
        private WorkerValidator _validator;

        public JobController(JobRepository jobRepository, WorkerValidator validator)
        {
            _jobRepository = jobRepository;
            _validator = validator;
        }

        // GET: api/values
        [HttpGet]
        public IActionResult Get()
        {
            return SuccessResult(_jobRepository.GetAllJobs());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var job = _jobRepository.GetJob(id);
            if (job == null)
                return NotFound($"Job with Id {id} not found");

            return SuccessResult(job);
        }

        [HttpGet]
        [Route("getJobsForDay")]
        public IActionResult GetJobsForDay(DateTime date)
        {
            return SuccessResult(_jobRepository.GetJobsForDay(date));
        }

        [HttpGet]
        [Route("getJobsForWeek")]
        public IActionResult GetJobsForWeek(DateTime date)
        {
            return SuccessResult(_jobRepository.GetJobsForWeek(date));
        }

        [HttpGet]
        [Route("getJobsForMonth")]
        public IActionResult GetJobsForMonth(DateTime date)
        {
            return SuccessResult(_jobRepository.GetJobsForMonth(date));
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]AddJobModel model)
        {
            try
            {
                Dictionary<Guid,IEnumerable<DateTime>> daysNotAvailable = new Dictionary<Guid, IEnumerable<DateTime>>();
                foreach( var guid in model.WorkerIds.Select(id => new Guid(id)))
                {
                    var working = _validator.GetDaysWorking(guid, model.StartDate, model.EndDate);
                    if (working.Any())
                        daysNotAvailable.Add(guid, working);
                }

                if( daysNotAvailable.Any())
                {
                    return BadRequest(new
                    {
                        message = "Failed to add job: Workers Unavailable",
                        data = daysNotAvailable
                    });
                }

                var job =_jobRepository.AddJob(model);
                return Ok(job);
            }
            catch
            {
                return BadRequest("Failed to add Job");
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            _jobRepository.DeleteJob(id);
        }
    }
}
