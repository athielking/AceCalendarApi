using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Identity;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Authorize]
    public class ApiBaseController : Controller
    {
        #region Properties  

        public AceUser AceUser
        {
            get;
            private set;
        }

        public Guid CalendarId
        {
            get;
            private set;
        }

        #endregion

        #region Data Members

        protected readonly UserManager<AceUser> _userManager;

        #endregion

        #region Constructor

        public ApiBaseController( UserManager<AceUser> userManager )
        {
            _userManager = userManager;
        }

        #endregion

        #region Protected Methods

        protected object GetErrorMessageObject( string errorMessage )
        {
            return new
            {
                errorMessage = errorMessage
            };
        }

        protected string GetModelStateErrors()
        {
            return String.Join(Environment.NewLine, ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage));
        }

        protected JsonResult PageOfDataJsonResult<TReturn>(int pageSize, IEnumerable<TReturn> data, int recordCount)
        {
            return new JsonResult(new
            {
                success = true,
                records = data,
                recordCount = recordCount,
                numberOfPages = (recordCount / pageSize) + 1
            });
        }

        protected JsonResult SuccessResult<TReturn>(IEnumerable<TReturn> data)
        {
            return new JsonResult(new
            {
                success = true,
                records = data
            });
        }

        protected JsonResult SuccessResult<TReturn>(TReturn data)
        {
            return new JsonResult(new
            {
                success = true,
                data = data
            });
        }

        protected bool UserIsAdmin()
        {
            return _userManager.GetRolesAsync( AceUser ).Result.Contains("Admin");
        }

        #endregion

        #region Overrides

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if(User.Identity.Name != null)
            {
                AceUser = _userManager.FindByNameAsync(User.Identity.Name).Result;

                if(Request.Headers.ContainsKey("SelectedCalendar"))
                {
                    CalendarId = new Guid(Request.Headers["SelectedCalendar"].First());
                }
            }

            base.OnActionExecuting(context);
        }

        #endregion
    }
}
