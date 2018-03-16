using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public class JobTags
    {
        public Guid Id { get; set; }
        public Guid IdJob { get; set; }
        public Guid IdTag { get; set; }

        public Job Job { get; set; }
        public Tag Tag { get; set; }
    }
}
