using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class EditTimeOffModel
    {
        #region Properties

        [Required]
        public Guid WorkerId
        {
            get;
            set;
        }

        [Required]
        public DateTime MonthDate
        {
            get;
            set;
        }

        [Required]
        public DateTime[] TimeOffDates
        {
            get;
            set;
        }

        #endregion
    }
}
