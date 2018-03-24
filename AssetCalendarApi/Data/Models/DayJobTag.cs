using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class DayJobTag
    {
        public Guid Id { get; set; }
        public Guid IdDayJob { get; set; }
        public Guid IdTag { get; set; }

        public DayJob DayJob { get; set; }
        public Tag Tag { get; set; }
    }
}
