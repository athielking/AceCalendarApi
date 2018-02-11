using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Data.Models
{
    public partial class Worker
    {
        #region Constructor

        public Worker()
        {
            DayJobWorkers = new HashSet<DayJobWorker>();
            DayOffWorkers = new HashSet<DayOffWorker>();
        }

        #endregion

        #region Properties

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        [JsonIgnore()]
        public ICollection<DayJobWorker> DayJobWorkers { get; set; }
        
        [JsonIgnore()]
        public ICollection<DayOffWorker> DayOffWorkers { get; set; }

        public Organization Organization { get; set; }

        public Guid OrganizationId
        {
            get;
            set;
        }

        #endregion
    }
}
