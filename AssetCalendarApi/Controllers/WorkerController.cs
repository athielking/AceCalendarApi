﻿using System;
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

        private readonly WorkerRepository _workerRepository;

        #endregion

        #region Constructor

        public WorkerController
        (
            WorkerRepository workerRepository,
            UserManager<CalendarUser> userManager
        ) : base(userManager)
        {
            _workerRepository = workerRepository;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var workers = _workerRepository.GetAllWorkers(CalendarUser.OrganizationId).ToList();

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
                var worker = _workerRepository.GetWorker(id, CalendarUser.OrganizationId);

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

        [HttpPost]
        public IActionResult Post([FromBody]WorkerViewModel worker)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var addedWorker = _workerRepository.AddWorker(worker, CalendarUser.OrganizationId);

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

                _workerRepository.EditWorker(id, worker, CalendarUser.OrganizationId);

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
                _workerRepository.DeleteWorker(id, CalendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Worker"));
            }
        }

        [HttpPost("addTimeOff")]
        public IActionResult AddTimeOff([FromBody]DateRangeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var worker = _workerRepository.AddTimeOff(model, CalendarUser.OrganizationId);

                return SuccessResult(worker.GetViewModel());

            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Time Off"));
            }
        }

        [HttpPost("deleteTimeOff")]
        public IActionResult DeleteTimeOff([FromBody]DateRangeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var worker = _workerRepository.DeleteTimeOff(model, CalendarUser.OrganizationId);

                return SuccessResult(worker.GetViewModel());

            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Time Off"));
            }
        }
        #endregion
    }
}
