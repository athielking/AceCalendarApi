using AssetCalendarApi.Data.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class OrganizationViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        //public string Email { get; set; }
        //public string Stripe_DefaultSourceId { get; set; }
        
        //public SubscriptionViewModel Subscription { get; set; }
        //public BillingInformationViewModel BillingInformation { get; set; }

        //public IEnumerable<UserViewModel> Users { get; set; }
        //public IEnumerable<Source> PaymentSources { get; set; }
    }
}
