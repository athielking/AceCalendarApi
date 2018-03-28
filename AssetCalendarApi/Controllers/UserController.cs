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
    public class UserController : ApiBaseController
    {

        private readonly OrganizationRepository _organizationRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        

        #region Constructor

        public UserController
        (
            OrganizationRepository organizationRepository,
            UserManager<CalendarUser> userManager,
            RoleManager<IdentityRole> roleManager
        ) : base(userManager)
        {
            _organizationRepository = organizationRepository;
            _roleManager = roleManager;
        }

        #endregion

        #region Public Methods

        
        [HttpPost]
        public IActionResult Post([FromBody]UserRequestModel user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var organization = _organizationRepository.GetOrganizationById(new Guid(user.OrganizationId));

                if (organization.Users.Any(u => u.UserName == user.UserName))
                    return BadRequest(GetErrorMessageObject("Username is already in use"));

                var result = _userManager.CreateAsync(new CalendarUser()
                {
                    OrganizationId = new Guid(user.OrganizationId),
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                }, user.Password).Result;

                if (!result.Succeeded)
                    return BadRequest(GetErrorMessageObject(result.Errors.First().Description));

                var addedUser = _userManager.FindByNameAsync(user.UserName).Result;

                if(_roleManager.RoleExistsAsync(user.Role).Result)
                    _userManager.AddToRoleAsync(addedUser, user.Role);
           
                return Ok(AutoMapper.Mapper.Map<UserViewModel>(addedUser));
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add Tag"));
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]UserRequestModel userModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var user = _userManager.FindByIdAsync(userModel.Id).Result;

                if (user == null)
                    return BadRequest(GetErrorMessageObject("Unable to locate user"));

                user.FirstName = userModel.FirstName;
                user.LastName = userModel.LastName;
                user.Email = userModel.Email;

                var result = _userManager.UpdateAsync(user).Result;
                if (!result.Succeeded)
                    return BadRequest(GetErrorMessageObject(result.Errors.First().Description));

                var roles = _userManager.GetRolesAsync(user).Result;
                if( !roles.Contains( userModel.Role ))
                {
                    result = _userManager.RemoveFromRolesAsync(user, roles).Result;
                    result = _userManager.AddToRoleAsync(user, userModel.Role).Result;
                }

                return Ok(userModel);
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Update User"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var user = _userManager.FindByIdAsync(id.ToString()).Result;
                var result = _userManager.DeleteAsync(user).Result;

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
