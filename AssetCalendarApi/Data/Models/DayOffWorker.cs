using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class DayOffWorker
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public Guid IdWorker { get; set; }

        public Worker Worker { get; set; }
    }
}
