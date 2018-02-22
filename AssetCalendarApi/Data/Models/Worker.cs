using AssetCalendarApi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public WorkerViewModel GetViewModel()
        {
            return new WorkerViewModel()
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Phone = this.Phone,
                TimeOff = this.DayOffWorkers.OrderBy(d=> d.Date).Select(d => d.Date),
                Jobs = this.DayJobWorkers.Select( djw => djw.DayJob ).Select( dj => dj.Job ).Distinct()
            };
        }
    }
}
