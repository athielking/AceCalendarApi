using AssetCalendarApi.Data.Models;
using AssetCalendarApi.Repository;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data
{
    public class AssetCalendarSeeder
    {
        #region Constants

        const string FiveZeroOneOrganizationName = "501Software";

        const string OrganizationOneName = "Organization1";

        const string OrganizationTwoName = "Organization2";

        private static class Roles
        {
            public static string Admin = "Admin";
            public static string User = "User";
            public static string Readonly = "Readonly";
        }
        #endregion

        #region Data Members

        private readonly AssetCalendarDbContext _assetCalendarDbContext;

        private readonly OrganizationRepository _organizationRepository;

        private readonly UserManager<CalendarUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        #endregion

        #region Constructor

        public AssetCalendarSeeder
        (
            AssetCalendarDbContext assetCalendarDbContext,
            UserManager<CalendarUser> userManager,
            RoleManager<IdentityRole> roleManager,
            OrganizationRepository organizationRepository
        )
        {
            _assetCalendarDbContext = assetCalendarDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _organizationRepository = organizationRepository;
        }

        #endregion

        #region Public Methods

        public async Task Seed()
        {
            _assetCalendarDbContext.Database.EnsureCreated();

            await SeedRoles();

            SeedOrganizations();

            await SeedUsers();

            SeedTags();
        }

        #endregion

        #region Private Methods

        private async Task SeedRoles()
        {
            if (!_roleManager.RoleExistsAsync(Roles.Admin).Result)
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            if (!_roleManager.RoleExistsAsync(Roles.User).Result)
                await _roleManager.CreateAsync(new IdentityRole(Roles.User));

            if (!_roleManager.RoleExistsAsync(Roles.Readonly).Result)
                await _roleManager.CreateAsync(new IdentityRole(Roles.Readonly));
        }

        private async Task SeedAdminUser()
        {
            var fiveZeroOneOrganization = _organizationRepository.GetOrganizationByName(FiveZeroOneOrganizationName);

            var user = await _userManager.FindByNameAsync("athielking");
            if( user == null )
            {
                user = new CalendarUser()
                {
                    FirstName = "Andrew",
                    LastName = "Thielking",
                    UserName = "athielking",
                    Email = "athielking@501software.com",
                    OrganizationId = fiveZeroOneOrganization.Id
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user");
                }

                await _userManager.AddToRoleAsync(user, Roles.Admin);
            }

            if (!_userManager.IsInRoleAsync(user, Roles.Admin).Result)
                await _userManager.AddToRoleAsync(user, Roles.Admin);


            user = await _userManager.FindByNameAsync("dculham");
            if (user == null)
            {
                user = new CalendarUser()
                {
                    FirstName = "David",
                    LastName = "Culham",
                    UserName = "dculham",
                    Email = "dculham@501software.com",
                    OrganizationId = fiveZeroOneOrganization.Id
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user");
                }

                await _userManager.AddToRoleAsync(user, Roles.Admin);
            }

            if (!_userManager.IsInRoleAsync(user, Roles.Admin).Result)
                await _userManager.AddToRoleAsync(user, Roles.Admin);


            user = await _userManager.FindByNameAsync("admin");

            if (user == null)
            {
                user = new CalendarUser()
                {
                    FirstName = "501",
                    LastName = "Software",
                    UserName = "admin",
                    OrganizationId = fiveZeroOneOrganization.Id
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user");
                }

                await _userManager.AddToRoleAsync(user, Roles.Admin);
            }

            if (!_userManager.IsInRoleAsync(user, Roles.Admin).Result)
                await _userManager.AddToRoleAsync(user, Roles.Admin);
        }

        private void SeedOrganizations()
        {
            var fiveZeroOneOrganization = _organizationRepository.GetOrganizationByName(FiveZeroOneOrganizationName);
            var organizationOne = _organizationRepository.GetOrganizationByName(OrganizationOneName);
            var organizationTwo = _organizationRepository.GetOrganizationByName(OrganizationTwoName);

            if (fiveZeroOneOrganization == null)
                _organizationRepository.AddOrganization(FiveZeroOneOrganizationName);

            if (organizationOne == null)
                _organizationRepository.AddOrganization(OrganizationOneName);

            if (organizationTwo == null)
                _organizationRepository.AddOrganization(OrganizationTwoName);
        }

        private async Task SeedUsers()
        {
            await SeedAdminUser();
            await SeedUserOne();
            await SeedUserTwo();
            await SeedUserThree();
        }

        private async Task SeedUserOne()
        {
            var user = await _userManager.FindByNameAsync("user1");

            var organizationOne = _organizationRepository.GetOrganizationByName(OrganizationOneName);

            if (user == null)
            {
                user = new CalendarUser()
                {
                    FirstName = "User",
                    LastName = "One",
                    UserName = "user1",
                    OrganizationId = organizationOne.Id
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user1");
                }
            }

            if (!_userManager.IsInRoleAsync(user, Roles.User).Result)
                await _userManager.AddToRoleAsync(user, Roles.User);
        }

        private async Task SeedUserTwo()
        {
            var user = await _userManager.FindByNameAsync("user2");

            var organizationTwo = _organizationRepository.GetOrganizationByName(OrganizationTwoName);

            if (user == null)
            {
                user = new CalendarUser()
                {
                    FirstName = "User",
                    LastName = "Two",
                    UserName = "user2",
                    OrganizationId = organizationTwo.Id
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user2");
                }
            }

            if (!_userManager.IsInRoleAsync(user, Roles.Readonly).Result)
                await _userManager.AddToRoleAsync(user, Roles.Readonly);

        }

        private async Task SeedUserThree()
        {
            var user = await _userManager.FindByNameAsync("user3");

            var organizationOne = _organizationRepository.GetOrganizationByName(OrganizationOneName);

            if (user == null)
            {
                user = new CalendarUser()
                {
                    FirstName = "User",
                    LastName = "Three",
                    UserName = "user3",
                    OrganizationId = organizationOne.Id
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user3");
                }
            }

            if (!_userManager.IsInRoleAsync(user, Roles.Readonly).Result)
                await _userManager.AddToRoleAsync(user, Roles.Readonly);

        }

        private void SeedTags()
        {
            var tag = _assetCalendarDbContext.Tags.FirstOrDefault(t => t.Icon == "airplanemode_active");
            if (tag == null)
            {
                _assetCalendarDbContext.Tags.Add(new Tag()
                {
                    Id = Guid.NewGuid(),
                    Icon = "airplanemode_active",
                    Description = "Out of Town",
                    Color = "#8BC34A"
                });
            }
        }

        #endregion
    }
}
