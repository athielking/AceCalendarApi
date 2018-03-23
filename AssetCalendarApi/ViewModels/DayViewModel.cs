using AssetCalendarApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class DayViewModel
    {
        public DateTime Date { get; set; }

        public IEnumerable<Job> Jobs { get; set; }
        public IEnumerable<Worker> AvailableWorkers { get; set; }
        public IEnumerable<Worker> TimeOffWorkers { get; set; }
        public IDictionary<Guid, IEnumerable<Worker>> WorkersByJob { get; set; }
        public IDictionary<Guid, IEnumerable<TagViewModel>> TagsByJob { get; set; }
    }
}
