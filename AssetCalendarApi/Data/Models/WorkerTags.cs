using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class WorkerTags
    {
        public Guid Id { get; set; }
        public Guid IdWorker { get; set; }
        public Guid IdTag { get; set; }

        public Worker Worker { get; set; }
        public Tag Tag { get; set; }
    }
}
