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

        #region Data Members

        private readonly OrganizationRepository _organizationRepository;

        private readonly RoleManager<IdentityRole> _roleManager;

        #endregion

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

        [HttpPost("addUserToOrganization/{id}")]
        public async Task<IActionResult> AddUserToOrganization(Guid id, [FromBody]AddUserModel addUserModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var organization = _organizationRepository.GetOrganizationWithUsers(id);

                if (organization.CalendarUsers.Any(u => u.UserName == addUserModel.Username))
                    return BadRequest(GetErrorMessageObject("Username is already in use"));

                var result = _userManager.CreateAsync(new CalendarUser()
                {
                    OrganizationId = id,
                    UserName = addUserModel.Username,
                    FirstName = addUserModel.FirstName,
                    LastName = addUserModel.LastName,
                    Email = addUserModel.Email,
                }, addUserModel.Password).Result;

                if (!result.Succeeded)
                    return BadRequest(GetErrorMessageObject(result.Errors.First().Description));

                var addedUser = _userManager.FindByNameAsync(addUserModel.Username).Result;

                if (_roleManager.RoleExistsAsync(addUserModel.Role).Result)
                    await _userManager.AddToRoleAsync(addedUser, addUserModel.Role);

                return Ok();
            }
            catch
            {
                return BadRequest(GetErrorMessageObject("Failed to Add User"));
            }
        }

        [HttpPut("editOrganizationUser/{id}")]
        public IActionResult EditOrganizationUser(Guid id, [FromBody]EditUserModel editUserModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var user = _userManager.FindByIdAsync(editUserModel.Id.ToString()).Result;
                var organization = _organizationRepository.GetOrganizationWithUsers(id);

                if (user == null)
                    return BadRequest(GetErrorMessageObject("Unable to locate user"));

                user.FirstName = editUserModel.FirstName;
                user.LastName = editUserModel.LastName;
                user.Email = editUserModel.Email;

                var result = _userManager.UpdateAsync(user).Result;
                if (!result.Succeeded)
                    return BadRequest(GetErrorMessageObject(result.Errors.First().Description));

                var roles = _userManager.GetRolesAsync(user).Result;
                if (!roles.Contains(editUserModel.Role))
                {
                    result = _userManager.RemoveFromRolesAsync(user, roles).Result;
                    result = _userManager.AddToRoleAsync(user, editUserModel.Role).Result;
                }

                return Ok();
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
                return BadRequest(GetErrorMessageObject("Failed to Delete User"));
            }
        }

        #endregion
    }
}
