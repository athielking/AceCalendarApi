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
using AssetCalendarApi.Tools;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AssetCalendarApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles="Admin, Organization Admin")]
    public class UserController : ApiBaseController
    {

        #region Data Members

        private readonly OrganizationRepository _organizationRepository;
        private readonly CalendarRepository _calendarRepository;
        private readonly SignalRService _signalRService;
        private readonly SendGridService _sendGridService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AceUser> _signInManager;

        #endregion

        #region Constructor

        public UserController
        (
            OrganizationRepository organizationRepository,
            CalendarRepository calendarRepository,
            SignalRService signalRService,
            SendGridService sendGridService,
            UserManager<AceUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AceUser> signInManager
        ) : base(userManager)
        {
            _organizationRepository = organizationRepository;
            _calendarRepository = calendarRepository;
            _signalRService = signalRService;
            _sendGridService = sendGridService;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        #endregion

        #region Public Methods

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterNewAccount([FromBody]RegisterUserViewModel model)
        {
            try
            {
                var addUserModel = model.UserModel;

                var user = await _userManager.FindByNameAsync(model.UserModel.Username);
                if (user != null)
                    return BadRequest(GetErrorMessageObject("Username is already in use."));

                user = await _userManager.FindByEmailAsync(model.UserModel.Email);
                if (user != null)
                    return BadRequest(new { errorMessage = "An account already exists for that email address", resendEmail = !user.EmailConfirmed, resetPassword = user.EmailConfirmed });

                var org = _organizationRepository.AddOrganization(model.OrganizationModel);
                _organizationRepository.StartTrial(org.Id);

                var result = _userManager.CreateAsync(new AceUser()
                {
                    OrganizationId = org.Id,
                    UserName = addUserModel.Username,
                    FirstName = addUserModel.FirstName,
                    LastName = addUserModel.LastName,
                    Email = addUserModel.Email,
                    EmailConfirmed = false
                }, addUserModel.Password).Result;

                if (!result.Succeeded)
                    return BadRequest(GetErrorMessageObject(result.Errors.First().Description));

                var addedUser = _userManager.FindByNameAsync(addUserModel.Username).Result;
                var calendar = _organizationRepository.AddCalendar(org.Id, "Default Calendar");

                _calendarRepository.AssignCalendarUsers(calendar.Id, new List<string>() { addedUser.Id });

                if (_roleManager.RoleExistsAsync(addUserModel.Role).Result)
                    await _userManager.AddToRoleAsync(addedUser, addUserModel.Role);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(addedUser);
                var safeToken = HttpUtility.UrlEncode(token);

                _sendGridService.SendEmailConfirmationEmail(addedUser, safeToken);

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(GetErrorMessageObject("Failed to Register User"));
            }
        }

        [AllowAnonymous]
        [HttpGet("register")]
        public async Task<IActionResult> ConfirmEmail(string userName, string code)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if( user == null )
                return BadRequest(GetErrorMessageObject($"Unable to locate user {userName} to confirm"));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
                return BadRequest(GetErrorMessageObject("Failed to confirm email."));



            return RedirectPermanent("https://app.acecalendar.io");
        }

        [AllowAnonymous]
        [HttpPost("resendEmail/{email}")]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user.EmailConfirmed)
                return Ok();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var safeToken = HttpUtility.UrlEncode(token);

            _sendGridService.SendEmailConfirmationEmail(user, safeToken);

            return Ok();
        }

        public async Task<IActionResult> ResetPassword(string userName, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(GetErrorMessageObject("An account with that email address does not exist"));

            if (user.UserName != userName)
                return BadRequest(GetErrorMessageObject("An account with that username and email combination does not exist"));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return Ok();
        }

        [HttpPost("addUserToOrganization/{id}")]
        public async Task<IActionResult> AddUserToOrganization(Guid id, [FromBody]AddUserModel addUserModel)
        {
            try
            {
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var organization = _organizationRepository.GetOrganizationWithUsers(id);

                if (organization.AceUsers.Any(u => u.UserName == addUserModel.Username))
                    return BadRequest(GetErrorMessageObject("Username is already in use"));

                if (UserViewModel.RoleIsEditor(addUserModel.Role))
                {
                    var license = _organizationRepository.GetSubscriptionLicenseDetails(id);
                    var users = _organizationRepository.GetOrganizationUsers(id).Where(u => u.IsEditor());

                    if (users.Count() >= license.EditingUsers && !UserIsAdmin())
                        return BadRequest(GetErrorMessageObject($"Cannot add user. Current subscription does not allow more than {license.EditingUsers} Editing Users"));
                }

                if (UserViewModel.RoleIsAdmin(addUserModel.Role) && !UserIsAdmin())
                    return BadRequest(GetErrorMessageObject("Only admin users can grant admin permissions"));

                var result = _userManager.CreateAsync(new AceUser()
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
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                if (!ModelState.IsValid)
                    return BadRequest(GetErrorMessageObject(GetModelStateErrors()));

                var user = _userManager.FindByIdAsync(editUserModel.Id.ToString()).Result;
                var userVm = AutoMapper.Mapper.Map<UserViewModel>(user);

                var organization = _organizationRepository.GetOrganizationWithUsers(id);

                if (user == null)
                    return BadRequest(GetErrorMessageObject("Unable to locate user"));

                if (!userVm.IsEditor() && UserViewModel.RoleIsEditor(editUserModel.Role))
                {
                    var license = _organizationRepository.GetSubscriptionLicenseDetails(id);
                    var users = _organizationRepository.GetOrganizationUsers(id).Where(u => u.IsEditor());

                    if (users.Count() >= license.EditingUsers && !UserIsAdmin())
                        return BadRequest(GetErrorMessageObject($"Cannot edit user. Current subscription does not allow more than {license.EditingUsers} Editing Users"));
                }

                if(userVm.IsAdmin() && !UserIsAdmin())
                    return BadRequest(GetErrorMessageObject("Only admin users can grant admin permissions"));

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

                _signalRService.CheckSubscriptionAsync(id);
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
                if (!UserIsAdmin() && AceUser.OrganizationId != id)
                    return BadRequest(GetErrorMessageObject($"User '{AceUser.UserName}' does not have access to this organization."));

                var user = _userManager.FindByIdAsync(id.ToString()).Result;
                var result = _userManager.DeleteAsync(user).Result;

                _signalRService.CheckSubscriptionAsync(id);
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
