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
            UserManager<AceUser> userManager
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
            if (CalendarId == null)
                return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));
            try { 
                var result = _calendarRepository.GetDataForRange(date, CalendarId, endDate, idWorker);
                _signalRService.CheckSubscriptionAsync(AceUser.OrganizationId);

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
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to Copy Calendar Day. Calendar Id not set"));

                _jobRepository.CopyCalendarDay(CalendarId, dateFrom, dateTo);
                _signalRService.SendDataUpdatedAsync(dateTo, CalendarId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Copy Calendar Day"));
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var calendars = _calendarRepository.GetCalendarsForOrganization(AceUser.OrganizationId);
                return SuccessResult(calendars);
            }
            catch (Exception ex)
            {
                return BadRequest(GetErrorMessageObject($"Failed Getting Organization Calendars: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            try
            {
                var calendar = _calendarRepository.GetCalendar(id);
                return SuccessResult(calendar);
            }
            catch( Exception ex )
            {
                return BadRequest(GetErrorMessageObject($"Failed Getting Calendar: {ex.Message}"));
            }
        }

        [HttpGet("users/{id}")]
        public IActionResult GetUsers(Guid id)
        {
            try
            {
                var calendar = _calendarRepository.GetCalendarUsers(id);
                return SuccessResult(calendar);
            }
            catch (Exception ex)
            {
                return BadRequest(GetErrorMessageObject($"Failed Getting Calendar: {ex.Message}"));
            }
        }

        [HttpPost("users/{id}")]
        public IActionResult AssignUsers(Guid id, [FromBody]List<string> users)
        {
            try
            {
                var calendarUsers = _calendarRepository.AssignCalendarUsers(id, users);

                foreach (var userId in users)
                    _signalRService.SendUserDataUpdatedAsync(userId);

                return SuccessResult(calendarUsers);
            }
            catch( Exception ex)
            {
                return BadRequest(GetErrorMessageObject($"Failed Assigning Users to Calendar: {ex.Message}"));
            }
        }

        [HttpDelete("user/{id}/{userId}")]
        public IActionResult DeleteUserFromCalendar(Guid id, string userId)
        {
            try
            {
                var calendarUsers = _calendarRepository.DeleteUserFromCalendar(id, userId);
                _signalRService.SendUserDataUpdatedAsync(userId);

                return SuccessResult(calendarUsers);
            }
            catch(Exception ex)
            {
                return BadRequest(GetErrorMessageObject($"Failed Deleting User from Calendar: {ex.Message}"));
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]AddCalendarModel model)
        {
            if (AceUser == null)
                return BadRequest(GetErrorMessageObject("Failed to add calendar. User is null"));

            try
            {
                var result = _calendarRepository.AddCalendarToOrganization(model);
                return SuccessResult(result);
            }
            catch( Exception ex)
            {
                return BadRequest(GetErrorMessageObject(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var calendars = _calendarRepository.DeleteCalendar(id);
                return SuccessResult(calendars);
            }
            catch (Exception ex)
            {
                return BadRequest(GetErrorMessageObject($"Failed Deleting Calendar: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]EditCalendarModel editCalendarModel)
        {
            try
            {
                _calendarRepository.EditCalendar(id, editCalendarModel);

                var calendarUsers = _calendarRepository.GetCalendarUsers(id);

                foreach (var calendarUser in calendarUsers)
                    _signalRService.SendUserDataUpdatedAsync(calendarUser.UserId);

                _signalRService.CheckSubscriptionAsync(AceUser.OrganizationId);

                return Ok();
            }
            catch(ApplicationException ex)
            {
                return BadRequest(GetErrorMessageObject(ex.Message));
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to update calendar"));
            }
        }

        [HttpGet("activateCalendarRecord/{organizationId}/{calendarId}")]
        public IActionResult ActivateCalendarRecord(Guid organizationId, Guid calendarId)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != organizationId)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _calendarRepository.ActivateCalendarRecord(calendarId);

                return Ok();
            }
            catch
            {
            }

            return BadRequest(GetErrorMessageObject("Failed to Activate Calendar Record"));
        }

        [HttpGet("inactivateCalendarRecord/{organizationId}/{calendarId}")]
        public IActionResult InactivateCalendarRecord(Guid organizationId, Guid calendarId)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != organizationId)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                _calendarRepository.InactivateCalendarRecord(calendarId);

                return Ok();
            }
            catch
            {
            }

            return BadRequest(GetErrorMessageObject("Failed to Inactivate Calendar Record"));
        }

        #endregion
    }
}
