using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class TagsByJobDate
    {
        public Guid Id { get; set; }
        public string MatIcon { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public Guid IdJob { get; set; }
        public DateTime Date { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
