using AssetCalendarApi.Data.Models;
using AssetCalendarApi.Filters;
using AssetCalendarApi.Repository;
using AssetCalendarApi.Tools;
using AssetCalendarApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AssetCalendarApi.Controllers
{
    public class AuthController : Controller
    {
        #region Data Members

        private ILogger<AuthController> _logger;

        private SignInManager<AceUser> _signInManager;

        private UserManager<AceUser> _userManager;

        private IConfiguration _configuration;

        private OrganizationRepository _organizationRepository;
        private CalendarRepository _calendarRepository;
        private SendGridService _sendGridService;

        #endregion

        #region Constructor

        public AuthController
        (
            SignInManager<AceUser> signInManager,
            UserManager<AceUser> userManager,
            OrganizationRepository organizationRepository,
            CalendarRepository calendarRepository,
            SendGridService sendGridService,
            ILogger<AuthController> logger,
            IConfiguration configuration
        )
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _organizationRepository = organizationRepository;
            _calendarRepository = calendarRepository;
            _sendGridService = sendGridService;
        }

        #endregion

        #region Public Methods

        [HttpPost("api/auth/logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while logging out: {ex}");
            }

            return BadRequest("Failed to logout");
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    return GenerateJwtToken(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while logging in: {ex}");
            }

            return BadRequest("Failed to login");
        }

        [HttpPost("api/auth/changePassword")]
        [ValidateModel]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel model)
        {
            try
            {
                var user = _userManager.FindByNameAsync(model.Username).Result;
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    user = _userManager.FindByNameAsync(model.Username).Result;
                    return GenerateJwtToken(user);
                }

                return BadRequest( result.Errors.FirstOrDefault().Description );
            }
            catch( Exception ex)
            {
                _logger.LogError($"Exception thrown while changing password: {ex}");
            }

            return BadRequest("Failed to Change Password");
        }

        [AllowAnonymous]
        [HttpPost("api/auth/resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordViewModel model)
        {

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return BadRequest(new { errorMessage = "Unable to locate user" });

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (!result.Succeeded)
                return BadRequest(new { errorMessage = result.Errors.First().Description });

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
                return GenerateJwtToken(user);

            return BadRequest(new { errorMessage = "Failed to sign in" });
        }

        [AllowAnonymous]
        [HttpGet("api/auth/resetPassword")]
        public async Task<IActionResult> ResetPassword(string userName, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new { errorMessage = "An account with that email address does not exist" });

            if (user.UserName != userName)
                return BadRequest(new { errorMessage = "An account with that username and email combination does not exist" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _sendGridService.SendPasswordResetEmail(user, HttpUtility.UrlEncode(token));

            return Ok();
        }

        [HttpGet("api/auth/validate/{id}")]
        public IActionResult ValidateSubscription(Guid id)
        {
            try
            {
                var validation = _organizationRepository.GetSubscriptionValidation(id);
                return new JsonResult( new { success = true, data = validation });
            }
            catch
            {
                return BadRequest(new
                {
                    errorMessage = "Failed to get Subscription Validation"
                });
            }
        }

        #endregion

        #region Private Methods

        public IActionResult GenerateJwtToken(AceUser user)
        {
            var validSubscription = _organizationRepository.GetSubscriptionValidation(user.OrganizationId);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? String.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? String.Empty),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("OrganizationId", user.OrganizationId.ToString()),
                new Claim("SubscriptionActive", validSubscription.IsValid.ToString() )
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            
            foreach(var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            var calendars = _calendarRepository.GetCalendarsForUser(user.Id, user.OrganizationId);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = user.UserName,
                organizationId = user.OrganizationId,
                calendars = calendars
            });
        }

        #endregion
    }
}
