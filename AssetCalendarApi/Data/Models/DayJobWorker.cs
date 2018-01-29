using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class DayJobWorker
    {
        #region Properties

        public Guid Id { get; set; }
        public Guid IdDayJob { get; set; }
        public Guid IdWorker { get; set; }

        public DayJob DayJob { get; set; }
        public Worker Worker { get; set; } 
        
        #endregion
    }
}
