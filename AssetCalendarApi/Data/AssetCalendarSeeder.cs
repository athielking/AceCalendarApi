using AssetCalendarApi.Data.Models;
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
        #region Data Members

        private readonly AssetCalendarDbContext _assetCalendarDbContext;

        private readonly UserManager<CalendarUser> _userManager;

        #endregion

        #region Constructor

        public AssetCalendarSeeder(AssetCalendarDbContext assetCalendarDbContext, UserManager<CalendarUser> userManager )
        {
            _assetCalendarDbContext = assetCalendarDbContext;
            _userManager = userManager;
        }

        #endregion

        #region Public Methods
        
        public async Task Seed()
        {
            _assetCalendarDbContext.Database.EnsureCreated();

            var user = await _userManager.FindByNameAsync("admin");

            if( user == null )
            {
                user = new CalendarUser()
                {
                    FirstName = "501",
                    LastName = "Software",
                    UserName = "admin"
                };

                var result = await _userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user");
                }
            }
        }

        #endregion
    }
}
