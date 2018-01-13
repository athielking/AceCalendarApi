using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class AddJobModel
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string[] WorkerIds { get; set; }

        public DateTime StartDate { get; set; }

        private int? _DurationDays;
        public int? DurationDays
        {
            get => _DurationDays;
            set
            {
                _DurationDays = value;

                if(value.HasValue)
                    EndDate = StartDate.AddDays(value.Value);
            }
        }

        private int? _DurationMonths;
        public int? DurationMonths
        {
            get => _DurationMonths;
            set
            {
                _DurationMonths = value;
                if (value.HasValue)
                    EndDate = StartDate.AddMonths(value.Value);
            }
        }

        private DateTime? _EndDate;
        public DateTime? EndDate
        {
            get => _EndDate;
            set
            {
                _EndDate = value;
                _DurationDays = null;
                _DurationMonths = null;
            }
        }
    }
}
