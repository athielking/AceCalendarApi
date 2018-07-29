using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class SubscriptionLicenseDetailsViewModel
    {
        #region Properties
        
        public int Calendars
        {
            get;
            set;
        }

        public int EditingUsers
        {
            get;
            set;
        }

        public int ReadonlyUsers
        {
            get;
            set;
        }

        #endregion
    }
}
