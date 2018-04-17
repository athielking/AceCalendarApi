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
        
        public string Id { get; set; }

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

        public IEnumerable<TagViewModel> Tags { get; set; }

        public IEnumerable<DateTime> JobDays { get; set; }
        #endregion
    }
}
