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

        #endregion

        #region Data Members

        private readonly AssetCalendarDbContext _assetCalendarDbContext;

        private readonly OrganizationRepository _organizationRepository;

        private readonly UserManager<CalendarUser> _userManager;

        #endregion

        #region Constructor

        public AssetCalendarSeeder
        (
            AssetCalendarDbContext assetCalendarDbContext, 
            UserManager<CalendarUser> userManager,
            OrganizationRepository organizationRepository
        )
        {
            _assetCalendarDbContext = assetCalendarDbContext;
            _userManager = userManager;
            _organizationRepository = organizationRepository;
        }

        #endregion

        #region Public Methods
        
        public async Task Seed()
        {
            _assetCalendarDbContext.Database.EnsureCreated();

            SeedOrganizations();

            await SeedUsers();
        }

        #endregion

        #region Private Methods

        private async Task SeedAdminUser()
        {
            var user = await _userManager.FindByNameAsync("admin");

            var fiveZeroOneOrganization = _organizationRepository.GetOrganizationByName(FiveZeroOneOrganizationName);

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
            }
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
        }

        #endregion
    }
}
