using AssetCalendarApi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetCalendarApi.Data.Models
{
    public partial class Worker
    {
        #region Properties

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        [JsonIgnore()]
        public ICollection<DayJobWorker> DayJobWorkers { get; set; }
        
        [JsonIgnore()]
        public ICollection<DayOffWorker> DayOffWorkers { get; set; }

        public Calendar Calendar { get; set; }

        public Guid CalendarId
        {
            get;
            set;
        }

        [JsonIgnore()]
        public ICollection<WorkerTags> WorkerTags { get; set; }

        #endregion

        #region Constructor

        public Worker()
        {
            DayJobWorkers = new HashSet<DayJobWorker>();
            DayOffWorkers = new HashSet<DayOffWorker>();
        }

        #endregion

        #region Public Methods

        public WorkerViewModel GetViewModel()
        {
            var workerViewModel = new WorkerViewModel()
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Phone = this.Phone
            };

            if (this.WorkerTags != null)
                workerViewModel.Tags = this.WorkerTags.Select(workerTag => workerTag.Tag.GetViewModel()).ToList();

            return workerViewModel;
        }
        
        #endregion
    }
}
