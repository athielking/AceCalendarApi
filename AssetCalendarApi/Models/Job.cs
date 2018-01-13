using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Models
{
    public partial class Job
    {
        public Job()
        {
            DaysJobs = new HashSet<DayJob>();
        }

        public Guid Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        [JsonIgnore]
        public ICollection<DayJob> DaysJobs { get; set; }
    }
}
