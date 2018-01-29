using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public partial class DayJob
    {
        #region Constructor

        public DayJob()
        {
            DayJobWorkers = new HashSet<DayJobWorker>();
        }

        #endregion

        #region Properties

        public Guid Id { get; set; }
        public Guid IdJob { get; set; }
        public DateTime Date { get; set; }

        public Job Job { get; set; }
        public ICollection<DayJobWorker> DayJobWorkers { get; set; } 
        
        #endregion
    }
}
