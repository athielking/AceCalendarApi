using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class JobController : ApiBaseController
    {
        private JobRepository _repository;

        public JobController(JobRepository repository)
        {
            _repository = repository;
        }

        // GET: api/values
        [HttpGet]
        public IActionResult Get()
        {
            return SuccessResult(_repository.GetAllJobs());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var job = _repository.GetJob(id);
            if (job == null)
                return NotFound($"Job with Id {id} not found");

            return SuccessResult(job);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]Job value)
        {
            try
            {
                _repository.AddJob(value, null);
                return Ok("Job Successfully added.");
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
        public void Delete(int id)
        {
        }
    }
}
