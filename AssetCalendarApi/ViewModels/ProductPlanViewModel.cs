using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class ProductPlanViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BillingScheme { get; set; }
        public decimal Amount { get; set; }
    }
}
