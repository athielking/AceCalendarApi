using AssetCalendarApi.Data.Models;
using AssetCalendarApi.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Tools
{
    public class JwtTokenService
    {
        private readonly UserManager<AceUser> _userManager;
        private readonly OrganizationRepository _organizationRepository;
        private readonly CalendarRepository _calendarRepository;
        private readonly IConfiguration _configuration;

        public JwtTokenService(
            UserManager<AceUser> userManager,
            OrganizationRepository organizationRepository,
            CalendarRepository calendarRepository,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _organizationRepository = organizationRepository;
            _calendarRepository = calendarRepository;
            _configuration = configuration;
        }

        public string GenerateJwtForUser(string userName)
        {
            var user = _userManager.FindByNameAsync(userName).Result;
            var validSubscription = _organizationRepository.GetSubscriptionValidation(user.OrganizationId);
            var roles = _userManager.GetRolesAsync(user).Result;

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

            foreach (var role in roles)
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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
