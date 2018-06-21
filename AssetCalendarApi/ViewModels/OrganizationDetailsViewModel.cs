using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class OrganizationDetailsViewModel
    {
        #region Properties

        public string AddressLine1
        {
            get;
            set;
        }

        public string AddressLine2
        {
            get;
            set;
        }

        public string OrganizationName
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public string City
        {
            get;
            set;
        }

        public string State
        {
            get;
            set;
        }

        public string Zip
        {
            get;
            set;
        }

        public string CityStateZip
        {
            get;
            set;
        }

        #endregion
    }
}
