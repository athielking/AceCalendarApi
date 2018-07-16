using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class CalendarUser
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid CalendarId { get; set; }

        public Calendar Calendar { get; set; }
    }
}
