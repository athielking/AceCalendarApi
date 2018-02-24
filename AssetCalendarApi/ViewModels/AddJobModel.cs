using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class AddJobModel
    {
        #region Properties

        public string Number
        {
            get;
            set;
        }

        [Required]
        public string Name
        {
            get;
            set;
        }

        public string Notes
        {
            get;
            set;
        }

        [Required]
        public DateTime StartDate
        {
            get;
            set;
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get
            {
                return _endDate ?? StartDate;
            }
            set
            {
                _endDate = value;
            }
        } 

        #endregion
    }
}
