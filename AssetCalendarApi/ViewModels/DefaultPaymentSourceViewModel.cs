using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class DefaultPaymentSourceViewModel
    {
        #region Properties

        public string Brand
        {
            get;
            set;
        }

        public string Last4
        {
            get;
            set;
        }

        public int ExpirationMonth
        {
            get;
            set;
        }

        public int ExpirationYear
        {
            get;
            set;
        }

        #endregion
    }
}
