using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class CalendarViewModel
    {
        public Guid Id
        {
            get;
            set;
        }

        public string CalendarName
        {
            get;
            set;
        }

        public Guid OrganizationId
        {
            get;
            set;
        }

        public bool Inactive
        {
            get;
            set;
        }
    }
}
