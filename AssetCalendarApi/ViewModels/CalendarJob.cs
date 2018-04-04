using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class CalendarJob
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        public IEnumerable<WorkerViewModel> Workers { get; set; }
        public IEnumerable<TagViewModel> jobTags { get; set; }
    }
}
