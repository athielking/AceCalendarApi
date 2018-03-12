using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class JobsByDateWorker
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public Guid OrganizationId { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; }
        public Guid IdWorker { get; set; }
    }
}
