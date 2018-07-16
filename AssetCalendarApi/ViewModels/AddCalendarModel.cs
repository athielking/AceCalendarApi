using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class AddCalendarModel
    {

        public string CalendarName
        {
            get;
            set;
        }

        public string OrganizationId
        {
            get;
            set;
        }

        public List<string> UserIds
        {
            get;
            set;
        }
    }
}
