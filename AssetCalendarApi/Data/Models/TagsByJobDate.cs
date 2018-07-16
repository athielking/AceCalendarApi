using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class TagsByJobDate
    {
        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public Guid IdJob { get; set; }
        public DateTime Date { get; set; }
        public Guid CalendarId { get; set; }
        public bool FromJobDay { get; set; }
    }
}
