using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class DateRangeViewModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? End { get; set; }
    }
}
