using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class Calendar
    {
        public Calendar()
        {
            Workers = new HashSet<Worker>();
            Jobs = new HashSet<Job>();
            CalendarUsers = new HashSet<CalendarUser>();
        }

        #region Properties

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

        public Organization Organization
        {
            get;
            set;
        }

        public ICollection<CalendarUser> CalendarUsers
        {
            get;
            set;
        }

        public ICollection<Worker> Workers
        {
            get;
            set;
        }

        public ICollection<Job> Jobs
        {
            get;
            set;
        }

        public ICollection<Tag> Tags
        {
            get;
            set;
        }

        #endregion

    }
}
