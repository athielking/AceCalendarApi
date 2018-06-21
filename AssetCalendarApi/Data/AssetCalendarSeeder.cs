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
        const string itsOrganizationName = "ITS LLC";

        const string OrganizationOneName = "Organization1";
        const string OrganizationTwoName = "Organization2";

        private static class Roles
        {
            public static string Admin = "Admin";
            public static string OrganizationAdmin = "Organization Admin";
            public static string User = "User";
            public static string Readonly = "Readonly";
        }

        #endregion

        #region Data Members

        private readonly AssetCalendarDbContext _assetCalendarDbContext;

        private readonly OrganizationRepository _organizationRepository;

        private readonly WorkerRepository _workerRepository;

        private readonly JobRepository _jobRepository;

        private readonly TagRepository _tagRepository;

        private readonly UserManager<CalendarUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        #endregion

        #region Constructor

        public AssetCalendarSeeder
        (
            AssetCalendarDbContext assetCalendarDbContext,
            UserManager<CalendarUser> userManager,
            RoleManager<IdentityRole> roleManager,
            OrganizationRepository organizationRepository,
            WorkerRepository workerRepository,
            JobRepository jobRepository,
            TagRepository tagRepository
        )
        {
            _assetCalendarDbContext = assetCalendarDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _organizationRepository = organizationRepository;
            _workerRepository = workerRepository;
            _jobRepository = jobRepository;
            _tagRepository = tagRepository;
        }

        #endregion

        #region Public Methods

        public async Task Seed()
        {
            _assetCalendarDbContext.Database.EnsureCreated();

            await SeedRoles();

            SeedOrganizations();

            await SeedUsers();

            //SeedCalendars();
        }

        #endregion

        #region Private Methods

        private async Task SeedRoles()
        {
            if (!_roleManager.RoleExistsAsync(Roles.Admin).Result)
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            if (!_roleManager.RoleExistsAsync(Roles.OrganizationAdmin).Result)
                await _roleManager.CreateAsync(new IdentityRole(Roles.OrganizationAdmin));

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
            //var fiveZeroOneOrganization = _organizationRepository.GetOrganizationByName(FiveZeroOneOrganizationName);
            //var itsOrganization = _organizationRepository.GetOrganizationByName(itsOrganizationName);
            

            //if (fiveZeroOneOrganization == null)
            //    _organizationRepository.AddOrganization(FiveZeroOneOrganizationName);

            //if (itsOrganization == null)
            //    _organizationRepository.AddOrganization(itsOrganizationName);
        }

        //private void SeedCalendars()
        //{
        //    foreach (var organization in _organizationRepository.GetAllOrganizations())
        //    {
        //        var calendars = _organizationRepository.GetOrganizationCalendars(organization.Id);

        //        if (calendars.Any())
        //            continue;

        //        SeedDefaultCalendar(organization);
        //    }
        //}

        //private void SeedDefaultCalendar(Organization organization)
        //{
        //    var calendar = _organizationRepository.AddCalendar(organization.Id, "Default Calendar", false, _assetCalendarDbContext );

        //    var workers = _workerRepository.GetWorkersByOrganization(organization.Id).ToList();

        //    foreach(var worker in workers)
        //    {
        //        if (worker.CalendarId == Guid.Empty)
        //            worker.CalendarId = calendar.Id;

        //        _assetCalendarDbContext.Workers.Update(worker);
        //    }

        //    var jobs = _jobRepository.GetAllJobs(organization.Id).ToList();

        //    foreach (var job in jobs)
        //    {
        //        if (job.CalendarId == Guid.Empty)
        //            job.CalendarId = calendar.Id;

        //        _assetCalendarDbContext.Jobs.Update(job);
        //    }

        //    var tags = _tagRepository.GetTagsByOrganization(organization.Id).ToList();

        //    foreach (var tag in tags)
        //    {
        //        if (tag.CalendarId == Guid.Empty)
        //            tag.CalendarId = calendar.Id;

        //        _assetCalendarDbContext.Tags.Update(tag);
        //    }

        //    //Make sure to first test without saving
        //    //_assetCalendarDbContext.SaveChanges();
        //}

        private async Task SeedUsers()
        {
            await SeedAdminUser();
            //await SeedITSUsers();
        }

        private async Task SeedITSUsers()
        {
            var itsAdmin = await _userManager.FindByNameAsync("itsAdmin");
            var itsUser = await _userManager.FindByNameAsync("itsUser");

            var itsOrganization = _organizationRepository.GetOrganizationByName(itsOrganizationName);

            if( itsAdmin == null)
            {
                itsAdmin = new CalendarUser()
                {
                    FirstName = "ITS Administrator",
                    UserName = "itsAdmin",
                    OrganizationId = itsOrganization.Id
                };

                var result = await _userManager.CreateAsync(itsAdmin, "P@ssw0rd!");
                if( result != IdentityResult.Success )
                {
                    throw new InvalidOperationException("Failed to create ITS Admin User");
                }

                if (!_userManager.IsInRoleAsync(itsAdmin, Roles.User).Result)
                    await _userManager.AddToRoleAsync(itsAdmin, Roles.User);
            }

            if( itsUser == null )
            {
                itsUser = new CalendarUser()
                {
                    FirstName = "ITS User",
                    UserName = "itsUser",
                    OrganizationId = itsOrganization.Id
                };

                var result = await _userManager.CreateAsync(itsUser, "R3@donly$");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create ITS User");
                }

                if (!_userManager.IsInRoleAsync(itsUser, Roles.Readonly).Result)
                    await _userManager.AddToRoleAsync(itsAdmin, Roles.Readonly);
            }
        }
      
        #endregion
    }
}
