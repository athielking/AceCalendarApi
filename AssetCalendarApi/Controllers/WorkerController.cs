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
    public class WorkerController : ApiBaseController
    {
        private WorkerRepository _repository;

        public WorkerController(WorkerRepository workerRepository)
        {
            _repository = workerRepository;
        }

        // GET: api/values
        [HttpGet]
        public IActionResult Get()
        {
            return SuccessResult(_repository.GetAllWorkers().ToList());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var worker = _repository.GetWorker(id);
            if (worker != null)
                return SuccessResult(worker);

            return NotFound($"Worker with Id {id} not found");
        }

        [HttpGet]
        [Route("getAvailableWorkers")]
        public IActionResult GetAvailableWorkers(DateTime date)
        {
            return SuccessResult(_repository.GetAvailableWorkersForDay(date).ToList());
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]Worker worker)
        {
            try
            {
                _repository.AddWorker(worker);
                return Ok("Worker Added Successfully");
            }
            catch(Exception ex )
            {
                return BadRequest("Failed To Add Worker");
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
