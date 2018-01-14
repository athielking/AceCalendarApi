using AssetCalendarApi.Data.Models;
using AssetCalendarApi.Filters;
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

        private IPasswordHasher<CalendarUser> _hasher;

        private IConfigurationRoot _config;

        #endregion

        #region Constructor

        public AuthController
        (
            SignInManager<CalendarUser> signInManager,
            UserManager<CalendarUser> userManager,
            IPasswordHasher<CalendarUser> hasher,
            ILogger<AuthController> logger,
            IConfigurationRoot config
        )
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _hasher = hasher;
            _config = config;
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
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while logging in: {ex}");
            }

            return BadRequest("Failed to login");
        }

        [ValidateModel]
        [HttpPost("api/auth/token")]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        var userClaims = await _userManager.GetClaimsAsync(user);

                        var claims = new[]
                        {
              new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
              new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
              new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
              new Claim(JwtRegisteredClaimNames.Email, user.Email)
            }.Union(userClaims);

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                          issuer: _config["Tokens:Issuer"],
                          audience: _config["Tokens:Audience"],
                          claims: claims,
                          expires: DateTime.UtcNow.AddMinutes(15),
                          signingCredentials: creds
                          );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while creating JWT: {ex}");
            }

            return BadRequest("Failed to generate token");
        } 

        #endregion
    }
}
