﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.Models
{
    public partial class DayJob
    {
        public DayJob()
        {
            DayJobWorkers = new HashSet<DayJobWorker>();
        }

        public Guid Id { get; set; }
        public Guid IdJob { get; set; }
        public DateTime Date { get; set; }

        public Job Job { get; set; }
        public ICollection<DayJobWorker> DayJobWorkers { get; set; }
    }
}
