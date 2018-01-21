﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AssetCalendarApi.Data.Models
{
    public partial class Worker
    {
        public Worker()
        {
            DayJobWorkers = new HashSet<DayJobWorker>();
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public ICollection<DayJobWorker> DayJobWorkers { get; set; }
    }
}