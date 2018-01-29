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
        }

        #endregion

        #region Properties

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public ICollection<DayJobWorker> DayJobWorkers { get; set; }

        public Organization Organization { get; set; }

        public Guid OrganizationId
        {
            get;
            set;
        }

        #endregion
    }
}
