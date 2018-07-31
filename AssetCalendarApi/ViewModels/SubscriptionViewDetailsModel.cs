using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class SubscriptionViewDetailsModel
    {
        #region Public Methods

        public string ProductName
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public bool IsTrial
        {
            get;
            set;
        }

        public int DaysLeft
        {
            get;
            set;
        }

        public bool CancelAtPeriodEnd
        {
            get;
            set;
        }

        public string SubscriptionId
        {
            get;
            set;
        }

        public DateTime? CurrentPeriodEnd
        {
            get;
            set;
        }

        public bool HasDefaultPaymentMethod
        {
            get;
            set;
        }

        public String DefaultPaymentMethod
        {
            get;
            set;
        }

        public int Calendars
        {
            get;
            set;
        }

        public int Users
        {
            get;
            set;
        }

        #endregion


    }
}
