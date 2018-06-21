﻿using AssetCalendarApi.Data.Models;
using AssetCalendarApi.Filters;
using AssetCalendarApi.Repository;
using AssetCalendarApi.ViewModels;
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

namespace AssetCalendarApi.Controllers
{
    public class AuthController : Controller
    {
        #region Data Members

        private ILogger<AuthController> _logger;

        private SignInManager<CalendarUser> _signInManager;

        private UserManager<CalendarUser> _userManager;

        private IConfiguration _configuration;

        private OrganizationRepository _organizationRepository;
        #endregion

        #region Constructor

        public AuthController
        (
            SignInManager<CalendarUser> signInManager,
            UserManager<CalendarUser> userManager,
            OrganizationRepository organizationRepository,
            ILogger<AuthController> logger,
            IConfiguration configuration
        )
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _organizationRepository = organizationRepository; 
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

        #endregion

        #region Private Methods

        private IActionResult GenerateJwtToken( CalendarUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? String.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? String.Empty),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("OrganizationId", user.OrganizationId.ToString()),
                new Claim("SubscriptionActive", _organizationRepository.HasActiveSubscription(user.OrganizationId).ToString() )
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

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = user.UserName,
                organizationId = user.OrganizationId
            });
        }

        #endregion
    }
}
