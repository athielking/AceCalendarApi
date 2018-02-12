using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using AssetCalendarApi.Data.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class WorkerController : ApiBaseController
    {
        #region Data Members

        private readonly UserManager<CalendarUser> _userManager;

        private readonly WorkerRepository _workerRepository;

        #endregion

        #region Constructor

        public WorkerController
        (
            WorkerRepository workerRepository, 
            UserManager<CalendarUser> userManager
        )
        {
            _workerRepository = workerRepository;
            _userManager = userManager;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                var workers = _workerRepository.GetAllWorkers(calendarUser.OrganizationId).ToList();

                return SuccessResult(workers);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Workers"));
            }
        }

        [HttpGet]
        [Route("getAvailableWorkers")]
        public async Task<IActionResult> GetAvailableWorkers(DateTime start, DateTime? end)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                return SuccessResult(_workerRepository.GetAvailableWorkers(calendarUser.OrganizationId, start, end).ToList());
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed To Get Available Workers"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]WorkerViewModel worker)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                var addedWorker = _workerRepository.AddWorker(worker, calendarUser.OrganizationId);

                return Ok(addedWorker);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Worker"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody]WorkerViewModel worker)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                _workerRepository.EditWorker(id, worker, calendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Update Worker"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var calendarUser = await _userManager.FindByNameAsync(User.Identity.Name);

                _workerRepository.DeleteWorker(id, calendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Worker"));
            }
        }

        #endregion
    }
}
