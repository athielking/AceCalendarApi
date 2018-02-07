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
        }
        
        #endregion

        #region Properties

        public Guid Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Notes { get; set; }

        [JsonIgnore]
        public ICollection<DayJob> DaysJobs { get; set; }

        public Organization Organization { get; set; }

        public Guid OrganizationId
        {
            get;
            set;
        }

        #endregion
    }
}
