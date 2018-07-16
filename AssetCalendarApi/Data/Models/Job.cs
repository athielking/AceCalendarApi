using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Data.Models
{
    public partial class Job
    {
        #region Constructor

        public Job()
        {
            DaysJobs = new HashSet<DayJob>();
            Tags = new HashSet<JobTags>();
        }
        
        #endregion

        #region Properties

        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        [JsonIgnore]
        public ICollection<DayJob> DaysJobs { get; set; }

        public Calendar Calendar { get; set; }

        public Guid CalendarId
        {
            get;
            set;
        }

        [JsonIgnore]
        public ICollection<JobTags> Tags { get; set; }
        #endregion
    }
}
