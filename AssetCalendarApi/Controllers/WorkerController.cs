using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;

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
        public IActionResult GetAvailableWorkers(DateTime start, DateTime? end)
        {
            return SuccessResult(_repository.GetAvailableWorkers(start, end).ToList());
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]WorkerViewModel worker)
        {
            try
            {
                worker = _repository.AddWorker(worker);
                return Ok( worker );
            }
            catch
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
        public void Delete(string id)
        {
            _repository.DeleteWorker(id);
        }
    }
}
