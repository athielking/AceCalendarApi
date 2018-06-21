using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class SubscriptionViewModel
    {
        public string PlanId { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public int LicenseQuantity { get; set; }
        public int? PlanAmount { get; set; }
        public string BillingScheme { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
    }
}
