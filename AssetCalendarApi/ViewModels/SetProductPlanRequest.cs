using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class SetProductPlanRequest
    {
        #region Properties

        public string PlanId
        {
            get;
            set;
        } 

        #endregion
    }
}
