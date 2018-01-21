using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class MoveWorkerRequestModel
    {
        public string IdWorker { get; set; }
        public string IdJob { get; set; }
        public DateTime? Date { get; set; }
    }
}
