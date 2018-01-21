using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Authorize]
    public class ApiBaseController : Controller
    {
        #region Protected Methods

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

        #endregion
    }
}
