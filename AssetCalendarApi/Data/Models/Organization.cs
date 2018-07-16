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
            AceUsers = new HashSet<AceUser>();
            Calendars = new HashSet<Calendar>();
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

        public string Email
        {
            get;
            set;
        }

        public string Stripe_CustomerId
        {
            get;
            set;
        }

        public ICollection<AceUser> AceUsers
        {
            get;
            set;
        }

        public ICollection<Calendar> Calendars
        {
            get;
            set;
        }

        #endregion
    }
}
