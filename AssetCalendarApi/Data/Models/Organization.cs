using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class Organization
    {
        #region Constructor

        public Organization()
        {
            CalendarUsers = new HashSet<CalendarUser>();
            Workers = new HashSet<Worker>();
            Jobs = new HashSet<Job>();
        }

        #endregion


        #region Properties

        public Guid Id
        {
            get;
            set;
        }

        public string Name
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

        #endregion
    }
}
