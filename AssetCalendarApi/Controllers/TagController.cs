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
            UserManager<CalendarUser> userManager
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
                var tags = _tagRepository.GetAllTags(CalendarUser.OrganizationId).OrderBy(tag => tag.Description).ToList();

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
                var tags = _tagRepository.GetJobTags(CalendarUser.OrganizationId).OrderBy(tag => tag.Description).ToList();

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
                var tags = _tagRepository.GetWorkerTags(CalendarUser.OrganizationId).OrderBy(tag => tag.Description).ToList();

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
                var tag = _tagRepository.GetTag(id, CalendarUser.OrganizationId);

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

                Tag addedTag = _tagRepository.AddTag(tag, CalendarUser.OrganizationId);

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

                _tagRepository.EditTag(id, tag, CalendarUser.OrganizationId);

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
                _tagRepository.DeleteTag(id, CalendarUser.OrganizationId);

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
