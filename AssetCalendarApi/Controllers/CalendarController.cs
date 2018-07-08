using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AssetCalendarApi.Tools;

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : ApiBaseController
    {
        #region Data Members

        private readonly CalendarRepository _calendarRepository;
        private readonly JobRepository _jobRepository;
        private readonly SignalRService _signalRService;

        #endregion

        #region Constructor

        public CalendarController
        (
            CalendarRepository calendarRepository,
            JobRepository jobRepository,
            SignalRService signalRService,
            UserManager<CalendarUser> userManager
        ): base(userManager)
        {
            _calendarRepository = calendarRepository;
            _jobRepository = jobRepository;
            _signalRService = signalRService;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        [Route("getMonth")]
        public IActionResult GetDataForMonth(DateTime date, Guid? idWorker)
        {
            try
            {
                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                //Need the calendar to show from sunday to saturday regardless of month
                DateTime monthStart = new DateTime(date.Year, date.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1).EndOfWeek();

                return SuccessResult(GetDataForRange(monthStart, monthEnd, idWorker));
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Data for Month"));
            }
        }

        [HttpGet]
        [Route("getWeek")]
        public IActionResult GetDataForWeek(DateTime date, Guid? idWorker)
        {
            try
            {
                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                DateTime weekStart = date.StartOfWeek();
                DateTime weekEnd = date.EndOfWeek();
              
                return SuccessResult(GetDataForRange(weekStart, weekEnd, idWorker));
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Data for Week"));
            }
        }

        [HttpGet]
        [Route("getDay")]
        public IActionResult GetDataForDay(DateTime date, Guid? idWorker)
        {
            try
            {
                if (date.Year < 1900)
                    return BadRequest("Invalid Date");

                return SuccessResult(GetDataForRange(date, null, idWorker));
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Get Data For Day"));
            }
        }

        [HttpGet]
        [Route("getRange")]
        public IActionResult GetDataForRange( DateTime date, DateTime? endDate, Guid? idWorker)
        {
            try
            {
                var result = _calendarRepository.GetDataForRange(date, CalendarUser.OrganizationId, endDate, idWorker);

                return SuccessResult(result);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Data for Range"));
            }
        }

        [HttpPost]
        [Route("copyCalendarDay")]
        public IActionResult CopyCalendarDay( DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                _jobRepository.CopyCalendarDay(CalendarUser.OrganizationId, dateFrom, dateTo);
                _signalRService.SendDataUpdatedAsync(dateTo, CalendarUser.OrganizationId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Copy Calendar Day"));
            }
        }

        #endregion
    }
}
