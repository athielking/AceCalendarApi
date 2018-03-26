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
    [Authorize(Roles="Admin")]
    public class OrganizationController : ApiBaseController
    {
        #region Data Members

        private readonly OrganizationRepository _organizationRepository;

        #endregion

        #region Constructor

        public OrganizationController
        (
            OrganizationRepository organizationRepository,
            UserManager<CalendarUser> userManager
        ) : base(userManager)
        {
            _organizationRepository = organizationRepository;
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var orgs = _organizationRepository.GetAllOrganizations();

                return SuccessResult(orgs);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Organizations"));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            try
            {
                var org = _organizationRepository.GetOrganizationById(id);

                return SuccessResult(org);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Get Worker"));
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]SaveOrganizationRequestModel organization)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var addedOrg = _organizationRepository.AddOrganization(organization.Name);

                return Ok(addedOrg);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Tag"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]SaveOrganizationRequestModel organization)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                _organizationRepository.EditOrganization(id, organization.Name);

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
                _organizationRepository.DeleteOrganization(id);

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
