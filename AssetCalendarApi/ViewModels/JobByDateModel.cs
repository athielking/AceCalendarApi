using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class JobByDateModel
    {
        #region Properties
        
        public DateTime Date
        {
            get;
            set;
        }

        public string JobName
        {
            get;
            set;
        }

        public string JobNumber
        {
            get;
            set;
        }

        #endregion
    }
}
