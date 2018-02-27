using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetCalendarApi.ViewModels
{
    public class MoveWorkerRequestModel
    {
        #region Properties

        public Guid IdWorker
        {
            get;
            set;
        }

        public Guid? IdJob
        {
            get;
            set;
        }

        public DateTime? Date
        {
            get;
            set;
        } 
        
        public bool AddWorkerToAllDaysOnAJob
        {
            get;
            set;
        }

        #endregion
    }
}
