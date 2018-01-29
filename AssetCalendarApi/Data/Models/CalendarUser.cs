using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class CalendarUser : IdentityUser
    {
        #region Properties

        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public Organization Organization
        {
            get;
            set;
        }

        public Guid OrganizationId
        {
            get;
            set;
        }

        #endregion
    }
}
