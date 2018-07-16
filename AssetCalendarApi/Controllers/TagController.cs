using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TagController : ApiBaseController
    {
        #region Data Members

        private readonly TagRepository _tagRepository;

        #endregion

        #region Constructor

        public TagController
        (
            TagRepository tagRepository,
            UserManager<AceUser> userManager
        ) : base(userManager)
        {
            _tagRepository = tagRepository;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var tags = _tagRepository.GetAllTags(CalendarId).OrderBy(tag => tag.Description).ToList();

                return SuccessResult(tags);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Tags"));
            }
        }

        [HttpGet]
        [Route("getJobTags")]
        public IActionResult GetJobTags()
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var tags = _tagRepository.GetJobTags(CalendarId).OrderBy(tag => tag.Description).ToList();

                return SuccessResult(tags);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Job Tags"));
            }
        }

        [HttpGet]
        [Route("getWorkerTags")]
        public IActionResult GetWorkerTags()
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var tags = _tagRepository.GetWorkerTags(CalendarId).OrderBy(tag => tag.Description).ToList();

                return SuccessResult(tags);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Worker Tags"));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                var tag = _tagRepository.GetTag(id, CalendarId);

                return SuccessResult(tag.GetViewModel());
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Tags"));
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]TagViewModel tag)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                Tag addedTag = _tagRepository.AddTag(tag, CalendarId);

                return Ok(addedTag);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Tag"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]TagViewModel tag)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _tagRepository.EditTag(id, tag, CalendarId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Update Tag"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                if (CalendarId == null)
                    return BadRequest(GetErrorMessageObject("Failed to retrieve data for calendar. Calendar Id not set"));

                _tagRepository.DeleteTag(id, CalendarId);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Delete Tags"));
            }
        }
        
        #endregion
    }
}
